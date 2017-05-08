using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.ServiceModel;
using Nebula.Core;
using System.ServiceModel.Channels;

namespace Nebula.Core.Elysium
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single, AddressFilterMode = AddressFilterMode.Any)]
    public class Master : IMasterServer
    {
        private ConcurrentDictionary<IPEndPoint, NodeReference> m_hKnownPeers;
        private ServiceHost                                     m_hHost;        

        public IPEndPoint LocalEndPoint { get; private set; }

        public Master()
        {
            m_hKnownPeers = new ConcurrentDictionary<IPEndPoint, NodeReference>();
        }

        [ConsoleUIMethod]
        public virtual void Start(int iPort)
        {
            m_hHost                 = new ServiceHost(this, new Uri($"net.tcp://localhost:{iPort}/ElysiumMasterServer"));
            NetTcpBinding hBinding  = new NetTcpBinding();
            hBinding.ReceiveTimeout = TimeSpan.MaxValue;
            hBinding.SendTimeout    = TimeSpan.MaxValue;
            hBinding.Security.Mode  = SecurityMode.None;

            m_hHost.AddServiceEndpoint(typeof(IMasterServer), hBinding, string.Empty);
            m_hHost.Open();
        }

        [ConsoleUIMethod]
        public virtual void Stop()
        {
            m_hHost.Close();
        }


        public void Register(int iPort)
        {
            IMasterServerCallback hCb = OperationContext.Current.GetCallbackChannel<IMasterServerCallback>();
            RemoteEndpointMessageProperty hEndPointProp = OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;

            NodeReference hNewNode = new NodeReference(hCb, new IPEndPoint(IPAddress.Parse(hEndPointProp.Address), iPort));

            m_hKnownPeers.TryAdd(hNewNode.RemoteEndPoint, hNewNode);

        }

        [ConsoleUIMethod]
        public IPEndPoint[] GetNodes(int iMaxNodes)
        {
            return m_hKnownPeers.Values.Take(iMaxNodes).Select(x => x.RemoteEndPoint).ToArray();
        }

        #region Nested Types
        private class NodeReference
        {
            public NodeReference(IMasterServerCallback hCb, IPEndPoint hEp)
            {
                Node            = hCb;
                RemoteEndPoint  = hEp;
            }

            public IMasterServerCallback Node { get; private set; }
            public IPEndPoint RemoteEndPoint { get; private set; }
        }

        #endregion

    }
}
