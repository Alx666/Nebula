using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Todo: add syntax tree features 
//https://github.com/dotnet/roslyn/wiki/Getting-Started-C%23-Syntax-Analysis
namespace REPL
{
    class Program
    {
        static void Main(string[] args)
        {           

            ScriptState<object> hScriptState    = null;
            ScriptOptions       hOptions        = ScriptOptions.Default;

            hOptions     = hOptions.AddReferences(typeof(System.Object).Assembly, typeof(System.Linq.Enumerable).Assembly);
            hOptions     = hOptions.AddImports("System");
            hOptions     = hOptions.AddImports("System.Linq");
            hOptions     = hOptions.AddImports("System.Collections.Generic");
            hScriptState = CSharpScript.RunAsync("using System.IO;", hOptions).Result;

            while (true)
            {
                try
                {
                    string sCmdLine = Console.ReadLine();

                    hScriptState = hScriptState.ContinueWithAsync(sCmdLine).Result;
                }
                catch (AggregateException hEx)
                {
                    hEx.InnerExceptions.ToList().ForEach(e => Console.WriteLine(e.Message));                    
                }
                catch (Exception hEx)
                {
                    Console.WriteLine(hEx.Message);
                }
            }          
        }
    }
}
