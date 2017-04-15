using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nebula.Core;
using System.Threading;

namespace Nebula.MasterServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Nebula.Core.MasterServer hMs = new Core.MasterServer();
            hMs.Start(40000);


            ConsoleUI hConsole = new ConsoleUI(hMs, "Nebula Master Server");
            hConsole.Run();


            Thread.CurrentThread.Join();
        }
    }
}
