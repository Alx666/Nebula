using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace REPL
{
    class Program
    {
        static void Main(string[] args)
        {
            ScriptState<object> hScriptState    = null;
            ScriptOptions       hOptions        = ScriptOptions.Default;

            hOptions = hOptions.AddReferences(typeof(System.Object).Assembly, typeof(System.Linq.Enumerable).Assembly);
            hOptions = hOptions.AddImports("System");
            hOptions = hOptions.AddImports("System.Linq");
            hOptions = hOptions.AddImports("System.Collections.Generic");

            Console.WriteLine("C# Command Shell " + Environment.Version + Environment.NewLine);

            while (true)
            {
                try
                {
                    Console.Write("> ");                    
                    string sCmdLine = Console.ReadLine();

                    if (hScriptState == null)
                    {
                        hScriptState = CSharpScript.RunAsync(sCmdLine, hOptions).Result;
                        
                    }
                    else
                    {
                        hScriptState = hScriptState.ContinueWithAsync(sCmdLine).Result;
                    }
                }
                catch (Exception hEx)
                {
                    Console.WriteLine(hEx.Message);
                }
            }          
        }
    }
}
