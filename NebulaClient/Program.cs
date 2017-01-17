using Nebula.Shared;
using System;
using System.ServiceModel;

namespace Nebula.Client
{
    class NebulaMasterServiceCB : INebulaMasterServiceCB
    {
        public string AddModule(byte[] hAssembly)
        {
            Console.WriteLine("Received AddModule Request");
            return "Done";
        }
      
        public NebulaModuleInfo[] ListModules()
        {
            Console.WriteLine("Received ListModules Request");
            return null;
        }

        public string RemoveModule(Guid vAssemblyId)
        {
            Console.WriteLine("Received RemoveModule Request" + vAssemblyId);
            return "Done";
        }
    }

    class Program 
    {
        static void Main(string[] args)
        {
            NetTcpBinding hBinding = new NetTcpBinding();
            EndpointAddress hAddr  = new EndpointAddress("net.tcp://localhost:28000/NebulaMasterService");

            DuplexChannelFactory<INebulaMasterService> hFactory = new DuplexChannelFactory<INebulaMasterService>(typeof(NebulaMasterServiceCB), hBinding, hAddr);
            
            INebulaMasterService hService = hFactory.CreateChannel(new InstanceContext(new NebulaMasterServiceCB()));
            (hService as ICommunicationObject).Faulted += OnFaulted;
            (hService as ICommunicationObject).Closed  += OnClosed;

            hService.Register(Environment.MachineName);
            
            System.Threading.Thread.CurrentThread.Join();
        }

        private static void OnClosed(object sender, EventArgs e)
        {
            Console.WriteLine("Closed");
        }

        private static void OnFaulted(object sender, EventArgs e)
        {
            Console.WriteLine("Faulted");
        }
    }
}
