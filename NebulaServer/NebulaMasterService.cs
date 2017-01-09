using Nebula.Shared;
using System;
using System.Collections.Concurrent;
using System.ServiceModel;
using System.Threading;

namespace Nebula.Server
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
    public class NebulaMasterService : INebulaMasterService, IDisposable
    {
        private ConcurrentDictionary<INebulaMasterServiceCB, NebulaClient>          m_hClients;
        private ServiceHost                                                         m_hHost;
        private static int                                                          m_iCounter;

        public event Action<NebulaClient> ClientFaulted;
        public event Action<string> ClientConnected;

        public NebulaMasterService()
        {
            m_hClients = new ConcurrentDictionary<INebulaMasterServiceCB, NebulaClient>();
        }

        public void Start(int iPort)
        {
            m_hClients.Clear();
            m_hHost = new ServiceHost(this, new Uri($"net.tcp://localhost:{iPort}/NebulaMasterService"));
            m_hHost.AddServiceEndpoint(typeof(INebulaMasterService), new NetTcpBinding(), "");
            m_hHost.Open();
        }

        public void Stop()
        {
            try
            {
                m_hHost.Close();                
            }
            catch (Exception)
            {
            }
            finally
            {
                m_hHost = null;
                m_hClients.Clear();
            }
        }

        #region ContractOperations

        public string Execute(int iClientId, string sData)
        {            
            try
            {
                return m_hClients[OperationContext.Current.GetCallbackChannel<INebulaMasterServiceCB>()].Callback.Execute(sData);
            }
            catch (Exception hEx)
            {
                return hEx.ToString();
            }
        }

        public void Register(string sMachineInfo)
        {
            OperationContext        hCurrent    = OperationContext.Current;            
            INebulaMasterServiceCB  hCb         = hCurrent.GetCallbackChannel<INebulaMasterServiceCB>();
     
            (hCb as ICommunicationObject).Faulted += OnFaulted;
                        
            m_hClients.TryAdd(hCb, new NebulaClient(Interlocked.Increment(ref m_iCounter), hCb));
            
            ClientConnected?.Invoke(sMachineInfo);
        }

        private void OnFaulted(object sender, EventArgs e)
        {
            INebulaMasterServiceCB hClient = sender as INebulaMasterServiceCB;
            
            NebulaClient hRemoved;
            if (m_hClients.TryRemove(hClient, out hRemoved))
            {
                ClientFaulted?.Invoke(hRemoved);
            }
            else
            {
                //TODO: accrocco temporaneo, prevedere sistema per la notifica dei  dei client mancanti
                Console.WriteLine("Attemping to remove client failed");
            }            
        }

        #endregion




        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.Stop();
                }
                disposedValue = true;
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion    
    }

}
