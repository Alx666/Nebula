using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using System.Threading.Tasks;

namespace Nebula.Core
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
    public abstract class Service<TIService, TIServiceCallback> : IBaseService
        where TIService         : IBaseService
        where TIServiceCallback : IBaseService
    {
        public IPEndPoint LocalEndPoint { get; private set; }

        private ServiceHost                                         m_hHost;
        private string                                              m_sUriAppend;
        private InstanceContext                                     m_hContext;

        protected ConcurrentDictionary<IPEndPoint, IBaseService>    m_hChannels;
        private BlockingCollection<IPEndPoint>                      m_hTryConnect;
        private Task                                                m_hConnectionTask;

        protected Service(string sUriAppenName)
        {
            m_sUriAppend            = sUriAppenName;
            m_hContext              = new InstanceContext(this);
            m_hTryConnect           = new BlockingCollection<IPEndPoint>();
            m_hChannels             = new ConcurrentDictionary<IPEndPoint, IBaseService>();
        }


        [ConsoleUIMethod]
        public virtual void Start(int iPort)
        {
            m_hHost                 = new ServiceHost(this, new Uri($"net.tcp://localhost:{iPort}/{m_sUriAppend}"));
            LocalEndPoint           = new IPEndPoint(IPAddress.Loopback, iPort);
            NetTcpBinding hBinding  = new NetTcpBinding();
            hBinding.Security.Mode  = SecurityMode.None;

            m_hHost.AddServiceEndpoint(typeof(TIService), hBinding, string.Empty);
            m_hHost.Open();            

            m_hConnectionTask       = new Task(ConnectionTask, TaskCreationOptions.LongRunning);
            m_hConnectionTask.Start();
        }

        [ConsoleUIMethod]
        public virtual void Stop()
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
            }
        }

        [ConsoleUIMethod]
        public void Connect(string sKnowAddr, int iPort)
        {
            m_hTryConnect.Add(new IPEndPoint(IPAddress.Parse(sKnowAddr), iPort));         
        }

        private void ConnectionTask()
        {
            NetTcpBinding hBinding                      = new NetTcpBinding();
            hBinding.Security.Mode                      = SecurityMode.None;
            DuplexChannelFactory<TIService> hFactory    = new DuplexChannelFactory<TIService>(this.GetType(), hBinding);

            while (true)
            {
                try
                {
                    IPEndPoint hCurrent = m_hTryConnect.Take();

                    if (m_hChannels.ContainsKey(hCurrent))
                        continue;

                    TIService hNewNode      = hFactory.CreateChannel(m_hContext, new EndpointAddress($"net.tcp://{hCurrent.Address}:{hCurrent.Port}/{m_sUriAppend}"));

                    using (OperationContextScope hScope = new OperationContextScope(hNewNode as IContextChannel))
                    {
                        OperationContextEx.Current.ListenerEndPoint = hCurrent;
                    }

                    (hNewNode as ICommunicationObject).Faulted  += OnChannelTerminated;
                    (hNewNode as ICommunicationObject).Closed   += OnChannelTerminated;

                    m_hChannels.TryAdd(hCurrent, hNewNode);

                    hNewNode.Notify(LocalEndPoint.Port).ToList().ForEach(a => m_hTryConnect.Add(a));
                }
                catch (Exception)
                {
                    //Don't crash the thread, event handler will take care of the bad channel
                }
            }
        }

        private void OnChannelTerminated(object sender, EventArgs e)
        {
            //IBaseService hService = sender as IBaseService;

            //if (m_hChannels.TryRemove(hService.RemoteEndPoint, out hService))
            //{
            //    (hService as ICommunicationObject).Faulted -= OnChannelTerminated;
            //    (hService as ICommunicationObject).Closed -= OnChannelTerminated;
            //}
        }

        #region Contract Operations

        public IPEndPoint[] GetNodes(int iMaxNodes, IPEndPoint[] hExclude) => m_hChannels.Keys.ToArray();

        //Todo: iListenPort may be different than real (attack attemp?)
        public IPEndPoint[] Notify(int iListenPort)
        {
            TIServiceCallback hCb                       = OperationContextEx.Current.GetCallbackChannel<TIServiceCallback>();
            IPEndPoint hRemoteEndPoint                  = OperationContextEx.Current.RemoteEndPoint;
            hRemoteEndPoint.Port                        = iListenPort;
            OperationContextEx.Current.ListenerEndPoint = hRemoteEndPoint;

            (hCb as ICommunicationObject).Closed       += OnChannelTerminated;
            (hCb as ICommunicationObject).Faulted      += OnChannelTerminated;

            //When a client arrives we have a callback reference for sure
            m_hChannels.TryAdd(hRemoteEndPoint, hCb);

            return m_hChannels.Keys.ToList().Where(ip => ip.Port != iListenPort).ToArray();
        }



        [ConsoleUIMethod]
        public int GetNodesCount()              => m_hChannels.Count;

        #endregion


        [ConsoleUIMethod]
        public IPEndPoint[] EnumerateNodes()    => m_hChannels.Keys.ToArray();        
    }
}
