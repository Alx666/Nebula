using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nebula.Shared
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
    public class NebulaMasterService<T> : INebulaMasterService, IDisposable where T : NebulaClient, new()
    {
        private ConcurrentDictionary<INebulaMasterServiceCB, T> m_hClients;
        private ServiceHost                                     m_hHost;
        private static int                                      m_iCounter;

        public event Action<T> ClientFaulted;
        public event Action<T> ClientConnected;
        public event Action<T, Guid, string> ModuleDataReceived;

        public NebulaMasterService()
        {
            m_hClients = new ConcurrentDictionary<INebulaMasterServiceCB, T>();
        }

        public void Start(int iPort)
        {
            m_hClients.Clear();
            m_hHost = new ServiceHost(this, new Uri($"net.tcp://localhost:{iPort}/NebulaMasterService"));

            NetTcpBinding hBinding = new NetTcpBinding();
            hBinding.Security.Mode = SecurityMode.None;
            //hBinding.MaxBufferSize      = 51200;
            //hBinding.MaxBufferPoolSize  = 0;
            hBinding.MaxReceivedMessageSize = 2147483647;

            m_hHost.AddServiceEndpoint(typeof(INebulaMasterService), hBinding, "");
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


        public void Register(string sMachineInfo, NebulaModuleInfo[] hModules)
        {
            OperationContext hCurrent = OperationContext.Current;
            INebulaMasterServiceCB hCb = hCurrent.GetCallbackChannel<INebulaMasterServiceCB>();

            T hClient;
            if (m_hClients.TryGetValue(hCb, out hClient))
            {
                hClient.Modules = hModules;
            }
            else
            {
                (hCb as ICommunicationObject).Faulted += OnFaulted;

                RemoteEndpointMessageProperty hRemoteProperty = (OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty);

                hClient = new T();
                hClient.Id = Interlocked.Increment(ref m_iCounter);
                hClient.Callback = hCb;
                hClient.Machine = sMachineInfo;
                hClient.Address = new IPEndPoint(IPAddress.Parse(hRemoteProperty.Address), hRemoteProperty.Port);
                hClient.Modules = hModules;

                m_hClients.TryAdd(hCb, hClient);
                ClientConnected?.Invoke(hClient);
            }
        }


        //TODO: gestire la sessione, se arriva prima questa chiamata di register (plausibile per moduli mal implementati)
        public void ModuleData(Guid vId, string sData)
        {
            OperationContext hCurrent = OperationContext.Current;
            INebulaMasterServiceCB hCb = hCurrent.GetCallbackChannel<INebulaMasterServiceCB>();

            T hClient;
            m_hClients.TryGetValue(hCb, out hClient);

            NebulaModuleInfo hModule = hClient.Modules.Where(m => m.Guid == vId).First();

            ModuleDataReceived?.Invoke(hClient, vId, sData);
        }

        private void OnFaulted(object sender, EventArgs e)
        {
            INebulaMasterServiceCB hClient = sender as INebulaMasterServiceCB;

            T hRemoved;
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



        #endregion
    }

}
