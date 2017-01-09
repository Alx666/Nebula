using System;
using System.Threading;

namespace Nebula.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            NebulaMasterService hService = new NebulaMasterService();
            hService.ClientFaulted   += OnClientFaulted;
            hService.ClientConnected += OnClientConnected;
            hService.Start(28000);

            Console.WriteLine("Nebula Server Started");
            Console.WriteLine("net.tcp://localhost:28000/NebulaMasterService");

            Console.ReadLine();

            hService.Stop();

            Thread.CurrentThread.Join();
        }

        private static void OnClientConnected(string obj)
        {
            Console.WriteLine("Client Connected: " + obj);
        }

        private static void OnClientFaulted(NebulaClient obj)
        {
            Console.WriteLine("Client Faulted: " + obj.Id);
        }
    }
}
