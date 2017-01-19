using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Nebula.Shared
{
    public class NebulaMasterServiceCB : INebulaMasterServiceCB, IDisposable
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

        private List<INebulaModule> m_hModules;

        public NebulaMasterServiceCB(string sAddr, int iPort)
        {
            NetTcpBinding                               hBinding    = new NetTcpBinding();
            hBinding.Security.Mode                                  = SecurityMode.None;
            EndpointAddress                             hAddr       = new EndpointAddress($"net.tcp://{sAddr}:{iPort}/NebulaMasterService");
            DuplexChannelFactory<INebulaMasterService>  hFactory    = new DuplexChannelFactory<INebulaMasterService>(typeof(NebulaMasterServiceCB), hBinding, hAddr);

            Service                                                 = hFactory.CreateChannel(new InstanceContext(this));

            AppDomain.CurrentDomain.AssemblyResolve                += OnAssemblyResolve;

            m_hModules                                              = new List<INebulaModule>();
           
            IEnumerable<INebulaModule> hModules                     = from  hA in Directory.GetFiles(Environment.CurrentDirectory, "*.dll").SafeSelect(hF => Assembly.UnsafeLoadFrom(hF))
                                                                      from  hT in hA.GetTypes()
                                                                      from  hI in hT.GetInterfaces()
                                                                      where hI == typeof(INebulaModule)
                                                                      select Activator.CreateInstance(hT) as INebulaModule;  //Manco dante porcoddio
                        
            foreach (INebulaModule hModule in hModules)
            {
                try
                {
                    hModule.Start(null);
                    m_hModules.Add(hModule);
                }
                catch (Exception)
                {
                    //Skip faulted modules
                }
            }
        }


        public List<NebulaModuleInfo> AddModule(byte[] hAssembly)
        {
            string sFileName = Guid.NewGuid().ToString() + ".dll";

            File.WriteAllBytes(sFileName, hAssembly);

            Assembly hTmp = Assembly.UnsafeLoadFrom(sFileName);

            IEnumerable<INebulaModule> hModules = from hM in hTmp.GetTypes()
                                                  from hI in hM.GetInterfaces()
                                                  where hI == typeof(INebulaModule)
                                                  select Activator.CreateInstance(hM) as INebulaModule;

            List<NebulaModuleInfo> hInstalledModules = new List<NebulaModuleInfo>();

            foreach (INebulaModule hModule in hModules)
            {
                try
                {                    
                    hModule.AssemblyInstalled(sFileName, Environment.CurrentDirectory);                    
                    hModule.Start(null);
                    hInstalledModules.Add(hModule.ModuleInfo);
                }
                catch (Exception hEx)
                {
                    hModule.LastError = hEx;
                }               
            }

            
            return hInstalledModules;
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

        #region private stuff

        //TODO: temp code, not used
        private Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            Console.WriteLine($"OnAssemblyResolve {sender}");
            return null;

            //string      sResName = args.Name + ".dll";
            //Assembly    hThisAssembly = Assembly.GetExecutingAssembly();
            //using (Stream sInput = hThisAssembly.GetManifestResourceStream(sResName))
            //{
            //    return sInput != null ? Assembly.Load(StreamToBytes(sInput)) : null;
            //}
        }

        //TODO: temp code, not used
        //private static byte[] StreamToBytes(Stream hInput)
        //{
        //    int iCapacity = hInput.CanSeek ? (int)hInput.Length : 0;

        //    using (MemoryStream hOutput = new MemoryStream(iCapacity))
        //    {
        //        int iReadLength;
        //        byte[] hBuffer = new byte[4096];

        //        do
        //        {
        //            iReadLength = hInput.Read(hBuffer, 0, hBuffer.Length);
        //            hOutput.Write(hBuffer, 0, iReadLength);
        //        }
        //        while (iReadLength != 0);

        //        return hOutput.ToArray();
        //    }
        //}

        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~NebulaMasterServiceCB() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion


    }
}
