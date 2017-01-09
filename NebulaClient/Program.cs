using Nebula.Shared;
using System;
using System.ServiceModel;

namespace Nebula.Client
{
    class NebulaMasterServiceCB : INebulaMasterServiceCB
    {
        public string Execute(string sBase64Assembly)
        {

            Console.WriteLine(sBase64Assembly);
            return sBase64Assembly;

            //try
            //{
            //    byte[] hAssemblyMem = Convert.FromBase64String(sBase64Assembly);

            //    Assembly hAsm = Assembly.Load(hAssemblyMem);

            //    var res = from hType in hAsm.ExportedTypes
            //              from hMethod in hType.GetMethods(BindingFlags.Static | BindingFlags.Public)
            //              where hType.IsClass && hMethod.Name == "Main"
            //              select new { Type = hType, Main = hMethod };

            //    StringBuilder hSb = new StringBuilder();

            //    res.ToList().ForEach(x => hSb.AppendLine(x.Main.Invoke(null, null).ToString()));

            //    return hSb.ToString();
            //}
            //catch (Exception hEx)
            //{
            //    return hEx.ToString();
            //}
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

            hService.Register("Ciao sono il primo");
            
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
