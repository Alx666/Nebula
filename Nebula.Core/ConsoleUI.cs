using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Nebula.Core
{

    //Todo: throw non critical exceptions for base-derived defined methods with same signature
    //Todo: throw non critical exceptions for non parsable parameters
    //Todo: inspect static methods
    //Todo: support method overload
    //Todo: inspect template instance methods
    //Todo: inspect template static methods
    public class ConsoleUI : IDisposable
    {
        public string Title { get { return Console.Title; } set { Console.Title = value; } }

        private Regex m_hExp;
        private char[] m_hSeparators;
        private Thread m_hKeyboardThread;
        private object m_hInstance;
        private Stopwatch m_hStopwatch;
        private Process m_hCurrentProcess;

        private List<MethodCall> m_hMethodsList;
        private StringBuilder m_hCurrentCommand;
        private List<string> m_hAutoCompleteList;
        private int m_iAutoCompleteIndex;
        private List<string> m_hHistoryList;
        private int m_iHistoryIndex;

        private const int m_iErrorFreq = 250;
        private const int m_iSuccessFreq = 400;
        private const int m_iBeepTime = 200;

        public ConsoleUI(object hTarget, string sWindowName)
        {
            m_hSeparators       = new char[] { ' ', '(' };
            m_hExp              = new Regex("((?<=\")(.*?)(?=\"))|(\\w[^\\s]*)");
            m_hInstance         = hTarget;
            m_hCurrentCommand   = new StringBuilder(1024);
            m_hHistoryList      = new List<string>();

            Console.Title = sWindowName;
            Console.ForegroundColor = ConsoleColor.Green;

            //Iterate on Target Inheritance Hierarchy and seek methods marked with the ConsoleUIMethod attribute
            m_hMethodsList = (from hT in m_hInstance.GetType().GetInheritanceHierarchy()
                              from hM in hT.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                              from hA in hM.GetCustomAttributes(false)
                              where hA as ConsoleUIMethod != null && hM.DeclaringType == hT
                              select new MethodCall(hM, hTarget)).ToList();

            //Add system methods from ConsoleUI
            m_hMethodsList.AddRange((from hM in this.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                     from hA in hM.GetCustomAttributes(true)
                                     where (hA as ConsoleUIMethod) != null
                                     select new MethodCall(hM, this)).ToList());


            //Remove overloads and duplicate attribute on virtuals (still not supported)
            ConsoleUI.CheckForErrors(ref m_hMethodsList);

            m_hCurrentProcess = Process.GetCurrentProcess();
            m_hStopwatch = new Stopwatch();

            this.EmitSound = true;
        }



        public void Dispose()
        {
            m_hCurrentProcess.Dispose();
        }

        public void Run()
        {
            m_hKeyboardThread = new Thread(new ThreadStart(KeyboardHandlerThread));
            m_hKeyboardThread.Start();
        }

        public void RunAndWait()
        {
            m_hKeyboardThread = new Thread(new ThreadStart(KeyboardHandlerThread));
            m_hKeyboardThread.Start();
            Thread.CurrentThread.Join(); //famola zozza
        }

        public void Stop()
        {
            m_hKeyboardThread.Abort();
        }

        [ConsoleUIMethod]
        public IEnumerable<MethodCall> Help()
        {
            return m_hMethodsList.OrderBy(hM => hM.Method.Name).ToList();
        }

        [ConsoleUIMethod]
        public void Cls()
        {
            Console.Clear();
            Console.CursorTop = 0;
            Console.CursorLeft = 0;
            m_hCurrentCommand = new StringBuilder();
        }

        #region Private Members
        private void KeyboardHandlerThread()
        {
            while (true)
            {
                ConsoleKeyInfo hInput = Console.ReadKey(true);

                if (hInput.Key == ConsoleKey.Tab)
                {
                    this.HandleTab();           //Autocomplete
                }
                else if (hInput.Key == ConsoleKey.LeftArrow)
                {
                    this.HandleLeftArrow();     //Change Cursor Position
                }
                else if (hInput.Key == ConsoleKey.RightArrow)
                {
                    this.HandleRightArrow();    //Change Cursor Position
                }
                else if (hInput.Key == ConsoleKey.UpArrow)
                {
                    this.HandleUpArrow();       //History Backward
                }
                else if (hInput.Key == ConsoleKey.DownArrow)
                {
                    this.HandleDownArrow();     //History Forward
                }
                else if (hInput.Key == ConsoleKey.Delete)
                {
                    //throw new NotImplementedException();
                }
                else if (hInput.Key == ConsoleKey.Backspace)
                {
                    this.HandleBackspace();     //Delete previous character
                }
                else if (hInput.Key == ConsoleKey.Enter)
                {
                    this.ExecuteCommand();      //Execute Command
                }
                else
                {
                    this.HandleKeyChar(hInput.KeyChar);
                }
            }
        }



        private void HandleDownArrow()
        {
            if (m_hHistoryList.Count == 0)
                return;

            if (m_iHistoryIndex == m_hHistoryList.Count - 1 || m_iHistoryIndex == -1)
                return;

            if (m_iHistoryIndex < m_hHistoryList.Count)
                m_iHistoryIndex++;

            this.FillLine(m_hHistoryList[m_iHistoryIndex]);
        }

        private void HandleUpArrow()
        {
            if (m_hHistoryList.Count == 0)
                return;

            if (m_iHistoryIndex == -1)
            {
                m_iHistoryIndex = m_hHistoryList.Count - 1;
                this.FillLine(m_hHistoryList[m_iHistoryIndex]);
            }
            else
            {
                if (m_iHistoryIndex > 0)
                    m_iHistoryIndex--;

                this.FillLine(m_hHistoryList[m_iHistoryIndex]);
            }
        }

        private void HandleTab()
        {
            MatchCollection hColl = m_hExp.Matches(m_hCurrentCommand.ToString());
            if (hColl.Count != 1)
                return;           //Do not complete we are inputting parameters  

            if (m_hAutoCompleteList == null)
            {
                m_hAutoCompleteList = (from hM in m_hMethodsList where hM.CallName.Contains(hColl[0].Value.ToLower()) orderby hM.Method.Name descending select hM.Method.Name).ToList();
                m_iAutoCompleteIndex = 0;
            }

            if (m_hAutoCompleteList.Count > 0)
            {
                this.FillLine(m_hAutoCompleteList[m_iAutoCompleteIndex]);
                m_iAutoCompleteIndex++;
                if (m_iAutoCompleteIndex >= m_hAutoCompleteList.Count)
                    m_iAutoCompleteIndex = 0;
            }
        }

        private void ResetAutocompletion()
        {
            m_hAutoCompleteList = null;
        }

        private void FillLine(string sText, ConsoleColor eColor)
        {
            Console.CursorLeft = 0;
            ConsoleUI.Write(sText, eColor);

            for (int i = Console.CursorLeft; i < m_hCurrentCommand.Length; i++)
                Console.Write(' ');


            m_hCurrentCommand = new StringBuilder();
            m_hCurrentCommand.Append(sText);
            Console.CursorLeft = m_hCurrentCommand.Length;
        }

        private void FillLine(string sText)
        {
            this.FillLine(sText, Console.ForegroundColor);
        }

        private void HandleRightArrow()
        {
            if (Console.CursorLeft < m_hCurrentCommand.Length)
                Console.CursorLeft++;
        }

        private void HandleLeftArrow()
        {
            if (Console.CursorLeft > 0)
                Console.CursorLeft--;
        }

        private void HandleBackspace()
        {
            if (Console.CursorLeft == 0)
                return;

            int iLastIndex = Console.CursorLeft;

            m_hCurrentCommand.Remove(Console.CursorLeft - 1, 1);
            Console.CursorLeft--;

            for (int i = Console.CursorLeft; i < m_hCurrentCommand.Length; i++)
            {
                Console.Write(m_hCurrentCommand[i]);
            }

            Console.Write(' ');
            Console.CursorLeft = iLastIndex - 1;

            this.ResetAutocompletion();
        }

        private void HandleKeyChar(char vC)
        {
            if (Console.CursorLeft == Console.BufferWidth - 1)
                return;

            if (Console.CursorLeft == m_hCurrentCommand.Length)
            {
                m_hCurrentCommand.Append(vC);
                Console.Write(vC);
            }
            else
            {
                int iLastIndex = Console.CursorLeft;

                m_hCurrentCommand.Insert(Console.CursorLeft, vC);
                Console.Write(vC);

                for (int i = Console.CursorLeft; i < m_hCurrentCommand.Length; i++)
                {
                    Console.Write(m_hCurrentCommand[i]);
                }

                Console.CursorLeft = iLastIndex + 1;
            }

            this.ResetAutocompletion();
        }

        private void ExecuteCommand()
        {
            string sCommand = m_hCurrentCommand.ToString().Trim();

            if (sCommand.Length == 0)
                return;

            MethodCall  hMethod = null;
            object      hResult;

            IntPtr hAffinity = m_hCurrentProcess.ProcessorAffinity;
            ProcessPriorityClass eCurrentClass = m_hCurrentProcess.PriorityClass;
            ThreadPriority ePriority = Thread.CurrentThread.Priority;

            try
            {
                hMethod = sCommand.FindCaller(m_hMethodsList).FirstOrDefault();

                if (hMethod == null)
                    throw new MissingMethodException(sCommand);

                this.FillLine(sCommand, ConsoleColor.Yellow);

                m_hStopwatch.Reset();
                m_hStopwatch.Start();

                hResult = hMethod.Invoke();

                m_hStopwatch.Stop();
                long lResult = m_hStopwatch.ElapsedMilliseconds;

                this.FillLine(sCommand, ConsoleColor.Green);

                Console.WriteLine();

                if (hResult is IEnumerable && !(hResult is string))
                {
                    IEnumerable hCollection = hResult as IEnumerable;

                    ConsoleUI.WriteLine(string.Format("Enumerating {0}", hResult.GetType().GetFriendlyName()), ConsoleColor.DarkGreen);
                    Console.WriteLine();
                    int iCount = 0;
                    foreach (object hItem in hCollection)
                    {
                        ConsoleUI.WriteLine(hItem.ToString(), ConsoleColor.DarkGreen);
                        iCount++;
                    }

                    ConsoleUI.WriteLine(string.Format("{0} Elements", iCount), ConsoleColor.DarkGreen);
                }
                else
                {
                    if (hResult != null)
                        ConsoleUI.WriteLine(hResult.ToString(), ConsoleColor.DarkGreen);
                }

                ConsoleUI.WriteLine(string.Format("{0} Ms", lResult), ConsoleColor.DarkGreen);
                m_hSoundEmitter.BeepSuccess();
            }
            catch (ArgumentException)
            {
                this.FillLine(sCommand, ConsoleColor.Red);
                Console.WriteLine();
                ConsoleUI.WriteLine("Bad Arguments, check function signature:", ConsoleColor.DarkRed);
                ConsoleUI.WriteLine(hMethod.Signature, ConsoleColor.DarkRed);
                m_hSoundEmitter.BeepError();
            }
            catch (TargetInvocationException hEx)
            {
                this.FillLine(sCommand, ConsoleColor.Red);
                Console.WriteLine();
                ConsoleUI.WriteLine(hEx.InnerException.ToString(), ConsoleColor.DarkRed);
                m_hSoundEmitter.BeepError();
            }
            catch (Exception hEx)
            {
                this.FillLine(sCommand, ConsoleColor.Red);
                Console.WriteLine();
                ConsoleUI.WriteLine(hEx.ToString(), ConsoleColor.DarkRed);
                m_hSoundEmitter.BeepError();
            }
            finally
            {
                m_hCurrentProcess.ProcessorAffinity = hAffinity;
                m_hCurrentProcess.PriorityClass = eCurrentClass;
                Thread.CurrentThread.Priority = ePriority;

                m_hHistoryList.Add(sCommand);
                m_iHistoryIndex = -1;
                Console.CursorLeft = 0;
                m_hCurrentCommand = new StringBuilder();
                this.ResetAutocompletion();
                Console.WriteLine();
            }
        }

        private static void WriteLine(string sMessage, ConsoleColor eColor)
        {
            ConsoleColor ePrev = Console.ForegroundColor;
            Console.ForegroundColor = eColor;
            Console.WriteLine(sMessage);
            Console.ForegroundColor = ePrev;
        }

        private static void Write(string sMessage, ConsoleColor eColor)
        {
            ConsoleColor ePrev = Console.ForegroundColor;
            Console.ForegroundColor = eColor;
            Console.Write(sMessage);
            Console.ForegroundColor = ePrev;
        }


        #endregion



        #region Nested Types



        #endregion

        #region Audio Command Notification

        private ISoundEmitter m_hSoundEmitter;
        public bool EmitSound
        {
            get
            {
                return m_hSoundEmitter is ConsoleSound;
            }

            set
            {
                if (value)
                    m_hSoundEmitter = new ConsoleSound();
                else
                    m_hSoundEmitter = new NoSound();
            }
        }


        private interface ISoundEmitter
        {
            void BeepSuccess();
            void BeepError();
        }

        private class ConsoleSound : ISoundEmitter
        {
            public void BeepError()
            {
                Console.Beep(m_iErrorFreq, m_iBeepTime);
            }

            public void BeepSuccess()
            {
                Console.Beep(m_iSuccessFreq, m_iBeepTime);
            }
        }

        private class NoSound : ISoundEmitter
        {
            public void BeepError()
            {

            }

            public void BeepSuccess()
            {

            }
        }

        #endregion

        #region Utilities
        private static void CheckForErrors(ref List<MethodCall> hMethodsList)
        {
            List<MethodCall> hToRemove = new List<MethodCall>();

            //Check for Attribute definition on virtual and overrides && ConsoleUIMethod on overloads
            foreach (MethodCall hToCall in hMethodsList)
            {

                //virtual and overrides
                MethodInfo hCurrent = hToCall.Method;
                MethodInfo hBase = hToCall.Method.GetBaseDefinition();
                MethodInfo hLast = hCurrent;

                while (hCurrent.DeclaringType != hBase.DeclaringType)
                {
                    object hAttribute = hBase.GetCustomAttributes(typeof(ConsoleUIMethod), false).SingleOrDefault();
                    if (hAttribute != null)
                        hLast = hBase;

                    hCurrent = hBase;
                    hBase = hBase.GetBaseDefinition();
                }

                if (hToCall.Method.DeclaringType != hLast.DeclaringType)
                {
                    hToRemove.Add(hToCall);

                    StringBuilder hSb = new StringBuilder();
                    hSb.AppendFormat("Warning: {0}{1}", hToCall.Signature, Environment.NewLine);
                    hSb.AppendLine("\tConsoleUIMethod on virtual and override");
                    ConsoleUI.WriteLine(hSb.ToString(), ConsoleColor.DarkYellow);
                }
            }

            for (int i = 0; i < hMethodsList.Count; i++)
            {
                MethodCall hToCall = hMethodsList[i];

                for (int k = 0; k < hMethodsList.Count; k++)
                {
                    MethodCall hOverload = hMethodsList[k];

                    if (hToCall.Method.DeclaringType == hOverload.Method.DeclaringType && hToCall.CallName == hOverload.CallName && hToCall != hOverload)
                    {
                        hToRemove.Add(hOverload);
                        hMethodsList.RemoveAt(k);
                        k--;
                        i--;

                        StringBuilder hSb = new StringBuilder();
                        hSb.AppendFormat("Warning: {0}{1}", hOverload.Signature, Environment.NewLine);
                        hSb.AppendLine("\tMethods Overload Not Supported");
                        ConsoleUI.WriteLine(hSb.ToString(), ConsoleColor.DarkYellow);
                    }
                }
            }


            hMethodsList = (hMethodsList.Except(hToRemove)).ToList();

        }

        #endregion
    }

    public class ConsoleUIMethod : Attribute
    {

    }

    public class MethodCall
    {
        public MethodInfo   Method      { get; private set; }
        public object       Instance    { get; private set; }
        public string       CallName    { get; private set; }
        public string       Signature   { get; private set; }

        public int          ParameterCount => m_hParameterConverter.Count;

        private List<IParser>       m_hParameterConverter;
        private object[]            m_hLastParameters;
        
        public MethodCall(MethodInfo hMethod, object hInstance)
        {
            Method                      = hMethod;
            Instance                    = hInstance;
            CallName                    = hMethod.Name.ToLower();            
            Signature                   = hMethod.GetSignature();
            m_hParameterConverter       = hMethod.GetParameters().Select(p => p.ParameterType.GetParser()).ToList();
        }

        public void Load(List<string> hParams)
        {
            m_hLastParameters = hParams.Skip(1).Select((s, i) => m_hParameterConverter[i].Parse(s)).ToArray();
        }

        public override string ToString()
        {
            return Signature;
        }

        public object Invoke()
        {
            return Method.Invoke(Instance, m_hLastParameters);
        }

    
    }

    #region Parsers
    public interface IParser
    {
        object Parse(string sInput);
    }

    class ParserInt32 : IParser
    {
        public object Parse(string sInput)
        {
            return Int32.Parse(sInput);
        }
    }

    class ParserInt64 : IParser
    {
        public object Parse(string sInput)
        {
            return Int64.Parse(sInput);
        }
    }

    class ParserUInt32 : IParser
    {
        public object Parse(string sInput)
        {
            return UInt32.Parse(sInput);
        }
    }

    class ParserUInt64 : IParser
    {
        public object Parse(string sInput)
        {
            return UInt64.Parse(sInput);
        }
    }

    class ParserSingle : IParser
    {
        public object Parse(string sInput)
        {
            return Single.Parse(sInput);
        }
    }

    class ParserDouble : IParser
    {
        public object Parse(string sInput)
        {
            return Double.Parse(sInput);
        }
    }

    class ParserDecimal : IParser
    {
        public object Parse(string sInput)
        {
            return Decimal.Parse(sInput);
        }
    }

    class ParserInt16 : IParser
    {
        public object Parse(string sInput)
        {
            return Int16.Parse(sInput);
        }
    }

    class ParserUInt16 : IParser
    {
        public object Parse(string sInput)
        {
            return UInt16.Parse(sInput);
        }
    }

    class ParserBoolean : IParser
    {
        public object Parse(string sInput)
        {
            return Boolean.Parse(sInput);
        }
    }

    class ParserChar : IParser
    {
        public object Parse(string sInput)
        {
            if (sInput.Length > 1)
                throw new FormatException();
            else if (string.IsNullOrEmpty(sInput))
                throw new ArgumentNullException();
            else
                return sInput[0];
        }
    }
    class ParserByte : IParser
    {
        public object Parse(string sInput)
        {
            return Byte.Parse(sInput);
        }
    }

    class ParserSByte : IParser
    {
        public object Parse(string sInput)
        {
            return SByte.Parse(sInput);
        }
    }

    class ParserDateTime : IParser
    {
        public object Parse(string sInput)
        {
            return DateTime.Parse(sInput);
        }
    }
    class ParserString : IParser
    {
        public object Parse(string sInput)
        {
            return sInput;
        }
    }

    #endregion

    public static class ConsoleUIExtensions
    {
        private static Regex m_hExp                         = new Regex("((?<=\")(.*?)(?=\"))|(\\w[^\\s]*)");

        private static ParserInt32      m_hParserInt32      = new ParserInt32();
        private static ParserInt64      m_hParserInt64      = new ParserInt64();
        private static ParserUInt32     m_hParserUInt32     = new ParserUInt32();
        private static ParserUInt64     m_hParserUInt64     = new ParserUInt64();
        private static ParserSingle     m_hParserSingle     = new ParserSingle();
        private static ParserDouble     m_hParserDouble     = new ParserDouble();
        private static ParserDecimal    m_hParserDecimal    = new ParserDecimal();
        private static ParserInt16      m_hParserInt16      = new ParserInt16();
        private static ParserUInt16     m_hParserUInt16     = new ParserUInt16();
        private static ParserBoolean    m_hParserBool       = new ParserBoolean();
        private static ParserChar       m_hParserChar       = new ParserChar();
        private static ParserByte       m_hParserByte       = new ParserByte();
        private static ParserSByte      m_hParserSByte      = new ParserSByte();
        private static ParserDateTime   m_hParserDateTime   = new ParserDateTime();
        private static ParserString     m_hParserString     = new ParserString();

        public static IParser GetParser(this Type hType)
        {
            switch (Type.GetTypeCode(hType))
            {
                case TypeCode.Boolean:  return m_hParserBool;
                case TypeCode.Char:     return m_hParserChar;
                case TypeCode.SByte:    return m_hParserSByte;
                case TypeCode.Byte:     return m_hParserByte;
                case TypeCode.Int16:    return m_hParserInt16;
                case TypeCode.UInt16:   return m_hParserUInt16;
                case TypeCode.Int32:    return m_hParserInt32;
                case TypeCode.UInt32:   return m_hParserUInt32;
                case TypeCode.Int64:    return m_hParserInt64;
                case TypeCode.UInt64:   return m_hParserUInt64;
                case TypeCode.Single:   return m_hParserSingle;
                case TypeCode.Double:   return m_hParserDouble;
                case TypeCode.Decimal:  return m_hParserDecimal;
                case TypeCode.DateTime: return m_hParserDateTime;
                case TypeCode.String:   return m_hParserString;
                default: throw new Exception();
            }
        }

        public static string GetSignature(this MethodInfo hMethod)
        {
            ParameterInfo[] hParams = hMethod.GetParameters();
            string sReturnType = hMethod.ReturnParameter.ParameterType.GetFriendlyName();
            string sParameters = hParams.Length == 0 ? string.Empty : (from hP in hParams select $"{hP.ParameterType.GetFriendlyName()} {hP.Name}").Aggregate((x, y) => x + ", " + y);
            return $"{sReturnType} {hMethod.Name}({sParameters})";
        }

        public static string GetFriendlyName(this Type hType)
        {
            if (hType.IsGenericType)
            {
                return string.Format("{0}<{1}>", hType.Name.Split('`')[0], string.Join(", ", hType.GetGenericArguments().Select(x => GetFriendlyName(x)).ToArray()));
            }
            else
            {
                return hType.Name;
            }
        }

        public static IEnumerable<Type> GetInheritanceHierarchy(this Type hType)
        {
            for (Type hCurrent = hType; hCurrent != null; hCurrent = hCurrent.BaseType)
                yield return hCurrent;
        }

        public static List<MethodCall> FindCaller(this string sCommand, List<MethodCall> hValidMethods)
        {
            MatchCollection hColl   = m_hExp.Matches(sCommand);
            
            List<string> hTokens = new List<string>();

            for (int i = 0; i < hColl.Count; i++)
            {
                string sToken = hColl[i].Value.Trim().ToLower();
                if (sToken != "")
                    hTokens.Add(sToken);
            }

            if (hTokens.Count == 0)
                throw new MissingMethodException();

            List<MethodCall> hMethods = new List<MethodCall>();

            for (int i = 0; i < hValidMethods.Count; i++)
            {
                MethodCall hCurrent = hValidMethods[i];
                if (hCurrent.CallName == hTokens[0] && hCurrent.ParameterCount == hTokens.Count - 1)
                {
                    try
                    {
                        hCurrent.Load(hTokens);
                        hMethods.Add(hCurrent);
                    }
                    catch (Exception)
                    {
                        //Skip current method
                    }
                }
            }

            return hMethods;
        }

        

        //private static object[] GetParameters(this MethodInfo hMethod, List<string> hCmd)
        //{
        //    ParameterInfo[] hParamInfo = hMethod.GetParameters();
        //    object[] hRes = new object[hParamInfo.Length];

        //    if (hParamInfo.Length != hCmd.Count - 1)
        //    {
        //        throw new ArgumentException(hMethod.ToString());
        //    }


        //    for (int i = 0; i < hParamInfo.Length; i++)
        //    {
        //        if (hParamInfo[i].ParameterType == typeof(string))
        //        {
        //            hRes[i] = hCmd[i + 1];
        //        }
        //        else
        //        {
        //            try
        //            {
        //                if (hParamInfo[i].ParameterType.IsEnum)
        //                    hRes[i] = Enum.Parse(hParamInfo[i].ParameterType, hCmd[i + 1]);
        //                else
        //                    hRes[i] = hParamInfo[i].ParameterType.InvokeMember("Parse", BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public, null, null, new object[] { hCmd[i + 1], CultureInfo.InvariantCulture });
        //            }
        //            catch (Exception)
        //            {
        //                try
        //                {
        //                    hRes[i] = hParamInfo[i].ParameterType.InvokeMember("Parse", BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public, null, null, new object[] { hCmd[i + 1] });
        //                }
        //                catch (Exception)
        //                {
        //                    throw new InvalidOperationException(string.Format("Can't parse argument number {0}: {1}", hParamInfo[i].Position, hParamInfo[i].Name));
        //                }
        //            }
        //        }
        //    }

        //    return hRes;
        //}
    }

}


