using Nebula.Shared;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
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
        public event Action<NebulaClient> ClientConnected;

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


        public void Register(string sMachineInfo)
        {
            OperationContext        hCurrent    = OperationContext.Current;            
            INebulaMasterServiceCB  hCb         = hCurrent.GetCallbackChannel<INebulaMasterServiceCB>();
     
            (hCb as ICommunicationObject).Faulted += OnFaulted;

            RemoteEndpointMessageProperty hRemoteProperty = (OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty);            

            NebulaClient            hClient               = new NebulaClient(Interlocked.Increment(ref m_iCounter), hCb, sMachineInfo, new IPEndPoint(IPAddress.Parse(hRemoteProperty.Address), hRemoteProperty.Port));

            m_hClients.TryAdd(hCb, hClient);            
            ClientConnected?.Invoke(hClient);
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
                throw new NotImplementedException();
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

        public string Execute(int iClientId, string sBinary)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

}
