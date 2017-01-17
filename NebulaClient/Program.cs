using Nebula.Shared;
using System;
using System.ServiceModel;

namespace Nebula.Client
{
    class NebulaMasterServiceCB : INebulaMasterServiceCB
    {
        #region Event Accessors

        public event EventHandler Faulted
        {  
            add  
            {
                (Service as ICommunicationObject).Faulted += value;
            }

            remove  
            {
                (Service as ICommunicationObject).Faulted -= value;
            }  
        }

        public event EventHandler Closed
        {
            add
            {
                (Service as ICommunicationObject).Closed += value;
            }

            remove
            {
                (Service as ICommunicationObject).Closed -= value;
            }
        }

        #endregion

        public INebulaMasterService Service { get; private set; }

        public NebulaMasterServiceCB(string sAddr, int iPort)
        {
            NetTcpBinding hBinding  = new NetTcpBinding();
            EndpointAddress hAddr   = new EndpointAddress($"net.tcp://{sAddr}:{iPort}/NebulaMasterService");

            DuplexChannelFactory<INebulaMasterService> hFactory = new DuplexChannelFactory<INebulaMasterService>(typeof(NebulaMasterServiceCB), hBinding, hAddr);

            Service = hFactory.CreateChannel(new InstanceContext(this));                        
        }

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
            Console.WriteLine("Received RemoveModule Request: " + vAssemblyId);
            return "Done";
        }
    }

    class Program 
    {
        static void Main(string[] args)
        {
            NebulaMasterServiceCB hCb = new NebulaMasterServiceCB("localhost", 28000);
            hCb.Closed  += OnClosed;
            hCb.Faulted += OnFaulted;


            hCb.Service.Register(Environment.MachineName);
            
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
