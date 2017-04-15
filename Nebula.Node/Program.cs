using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nebula.Core;
using System.Threading;

namespace Nebula.Node
{
    class NebulaNode
    {
        static void Main(string[] args)
        {
            try
            {
                NodeInstance hInstance = new NodeInstance();
                int iPort;
                if (args.Length > 0 && int.TryParse(args[0], out iPort))
                {
                    hInstance.Start(iPort);
                }
                 
                ConsoleUI hConsole = new ConsoleUI(hInstance, "Nebula Node");

                hConsole.Run();
                Thread.CurrentThread.Join();
            }
            catch (Exception)
            {
            }
        }
    }
}
