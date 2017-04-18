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

        //Todo: create an adapter to handle both inbound & outbound messages (class ServicePair?)
        private ConcurrentDictionary<TIService, IPEndPoint>         m_hNodes;
        private ConcurrentDictionary<TIServiceCallback, IPEndPoint> m_hNodesCallback;
        private ConcurrentDictionary<IPEndPoint, IBaseService>      m_hChannels;

        private BlockingCollection<IPEndPoint>                      m_hAvaiableNodes;
        private Task                                                m_hConnectionTask;

        protected Service(string sUriAppenName)
        {
            m_sUriAppend        = sUriAppenName;
            m_hContext          = new InstanceContext(this);
            m_hNodes            = new ConcurrentDictionary<TIService, IPEndPoint>();
            m_hNodesCallback    = new ConcurrentDictionary<TIServiceCallback, IPEndPoint>();
            m_hAvaiableNodes    = new BlockingCollection<IPEndPoint>();
            m_hChannels         = new ConcurrentDictionary<IPEndPoint, IBaseService>();
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
            //(m_hHost.ChannelDispatchers[0] as ChannelDispatcher).ChannelInitializers.Add(this);

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

        //public void Initialize(IClientChannel hChannel)
        //{
            
        //}

        [ConsoleUIMethod]
        public void Connect(string sKnowAddr, int iPort)
        {
            m_hAvaiableNodes.Add(new IPEndPoint(IPAddress.Parse(sKnowAddr), iPort));         
        }

        private void ConnectionTask()
        {
            NetTcpBinding hBinding                      = new NetTcpBinding();
            hBinding.Security.Mode                      = SecurityMode.None;
            DuplexChannelFactory<TIService> hFactory    = new DuplexChannelFactory<TIService>(this.GetType(), hBinding);
            Dictionary<IPEndPoint, TIService> hFilter   = new Dictionary<IPEndPoint, TIService>();

            while (true)
            {
                IPEndPoint hCurrent             = m_hAvaiableNodes.Take();

                if (m_hChannels.ContainsKey(hCurrent))
                    continue;

                TIService hNewNode              = hFactory.CreateChannel(m_hContext, new EndpointAddress($"net.tcp://{hCurrent.Address}:{hCurrent.Port}/{m_sUriAppend}"));

                hNewNode.Notify(LocalEndPoint.Port).ToList().ForEach(a => m_hAvaiableNodes.Add(a));

                m_hChannels.TryAdd(hCurrent, hNewNode);
            }
        }

        #region Contract Operations

        public IPEndPoint[] GetNodes(int iMaxNodes, IPEndPoint[] hExclude) => m_hChannels.Keys.ToArray();

        public IPEndPoint[] Notify(int iListenPort)
        {
            TIServiceCallback hCb       = OperationContext.Current.GetCallbackChannel<TIServiceCallback>();
            IPEndPoint hRemoteEndPoint  = OperationContext.Current.GetRemoteEndPoint();
            hRemoteEndPoint.Port        = iListenPort;

            //When a client arrives we have a callback reference for sure
            m_hChannels.TryAdd(hRemoteEndPoint, hCb);

            return m_hChannels.Keys.ToList().Where(ip => ip.Port != iListenPort).ToArray();
        }

        [ConsoleUIMethod]
        public int GetNodesCount()                                         => m_hChannels.Count;

        #endregion


        [ConsoleUIMethod]
        public IPEndPoint[] EnumerateNodes()                               => m_hChannels.Keys.ToArray();
        




    }
}
