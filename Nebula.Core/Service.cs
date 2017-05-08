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
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single, AddressFilterMode = AddressFilterMode.Any)]
    public abstract class Service<TIService, TIServiceCallback> : IBaseService
        where TIService         : class, IBaseService
        where TIServiceCallback : class, IBaseService
    {
        public IPEndPoint LocalEndPoint { get; private set; }

        protected   ServiceHost                                         m_hHost;
        protected   DuplexChannelFactory<TIService>                     m_hServiceFactory;
        protected   InstanceContext                                     m_hContext;
        protected   NetTcpBinding                                       m_hNetTcpBinding;

        private     string                                              m_sUriAppend;
        private     ConcurrentDictionary<IPEndPoint, IBaseService>      m_hChannels;
        private     BlockingCollection<IPEndPoint>                      m_hTryConnect;
        private     Task                                                m_hConnectionTask;

        public event Action<IBaseService>                               ChannelClosed;
        public event Action<IBaseService>                               ChannelFauled;

        protected Service(string sUriAppenName)
        {
            m_sUriAppend                    = sUriAppenName;
            m_hContext                      = new InstanceContext(this);
            m_hTryConnect                   = new BlockingCollection<IPEndPoint>();
            m_hChannels                     = new ConcurrentDictionary<IPEndPoint, IBaseService>();
            m_hNetTcpBinding                = new NetTcpBinding();
            m_hNetTcpBinding.Security.Mode  = SecurityMode.None;
            m_hNetTcpBinding.SendTimeout    = TimeSpan.MaxValue;
            m_hNetTcpBinding.ReceiveTimeout = TimeSpan.MaxValue;
        }

        [ConsoleUIMethod]
        public virtual void Start(int iPort)
        {            
            m_hHost                 = new ServiceHost(this, new Uri($"net.tcp://localhost:{iPort}/{m_sUriAppend}"));
            LocalEndPoint           = new IPEndPoint(IPAddress.Loopback, iPort);                        
            m_hServiceFactory       = new DuplexChannelFactory<TIService>(this.GetType(), m_hNetTcpBinding);

            this.OnAddService();
            m_hHost.Open();            

            m_hConnectionTask       = new Task(ConnectionTask, TaskCreationOptions.LongRunning);
            m_hConnectionTask.Start();
        }

        protected virtual void OnAddService()
        {
            NetTcpBinding hBinding  = new NetTcpBinding();
            hBinding.ReceiveTimeout = TimeSpan.MaxValue;
            hBinding.SendTimeout    = TimeSpan.MaxValue;            
            hBinding.Security.Mode  = SecurityMode.None;

            m_hHost.AddServiceEndpoint(typeof(TIService), hBinding, string.Empty);
        }

        [ConsoleUIMethod]
        public virtual void Stop()
        {
            try
            {
                m_hHost.Close();
                m_hServiceFactory.Close();
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
            while (true)
            {
                try
                {
                    IPEndPoint hCurrent = m_hTryConnect.Take();

                    if (m_hChannels.ContainsKey(hCurrent))
                        continue;

                    TIService hNewNode      = m_hServiceFactory.CreateChannel(m_hContext, new EndpointAddress($"net.tcp://{hCurrent.Address}:{hCurrent.Port}/{m_sUriAppend}"));

                    using (OperationContextScope hScope = new OperationContextScope(hNewNode as IContextChannel))
                    {
                        OperationContextEx.Current.ListenerEndPoint = hCurrent;
                    }

                    (hNewNode as ICommunicationObject).Faulted  += OnChannelFaulted;
                    (hNewNode as ICommunicationObject).Closed   += OnChannelClosed;

                    m_hChannels.TryAdd(hCurrent, hNewNode);

                    hNewNode.Notify(LocalEndPoint.Port).ToList().ForEach(a => m_hTryConnect.Add(a));
                }
                catch (Exception)
                {
                    //Don't crash the thread, event handler will take care of the bad channel
                }
            }
        }

        //Guru Trick
        protected List<dynamic> Nodes => m_hChannels.Values.Select(c => c as dynamic).ToList();


        private void HandleDisconnection(IBaseService hService)
        {
            KeyValuePair<IPEndPoint, IBaseService> hRef = (from c in m_hChannels where c.Value == hService select c).FirstOrDefault();

            if (hRef.Key != null)
                m_hChannels.TryRemove(hRef.Key, out hService);

            if (hService != null)
            {
                (hService as ICommunicationObject).Faulted -= OnChannelFaulted;
                (hService as ICommunicationObject).Closed  -= OnChannelClosed;
            }
        }

        private void OnChannelFaulted(object sender, EventArgs e)
        {
            HandleDisconnection(sender as IBaseService);
            ChannelFauled?.Invoke(sender as IBaseService);
        }

        private void OnChannelClosed(object sender, EventArgs e)
        {
            HandleDisconnection(sender as IBaseService);
            ChannelClosed?.Invoke(sender as IBaseService);
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

            (hCb as ICommunicationObject).Closed       += OnChannelClosed;
            (hCb as ICommunicationObject).Faulted      += OnChannelFaulted;

            
            //When a client arrives we have a callback reference for sure
            m_hChannels.TryAdd(hRemoteEndPoint, hCb as IBaseService); //i know.. for now just hack

            return m_hChannels.Keys.ToList().Where(ip => ip.Port != iListenPort).ToArray();
        }



        [ConsoleUIMethod]
        public int GetNodesCount()              => m_hChannels.Count;

        #endregion


        [ConsoleUIMethod]
        public IPEndPoint[] EnumerateNodes()    => m_hChannels.Keys.ToArray();        
    }
}
