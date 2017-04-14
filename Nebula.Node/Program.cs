using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nebula.Core;
using Nebula.Node.UI;
using System.Threading;

namespace Nebula.Node
{
    class NebulaNode
    {
        static void Main(string[] args)
        {
            NodeInstance hInstance  = new NodeInstance();
            ConsoleUI hConsole      = new ConsoleUI(hInstance, "Nebula Node");            

            hConsole.Run();
            Thread.CurrentThread.Join();
        }
    }
}
