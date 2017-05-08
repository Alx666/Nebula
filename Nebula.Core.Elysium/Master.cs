using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.ServiceModel;
using Nebula.Core;
using System.ServiceModel.Channels;
using System.Collections.Generic;

namespace Nebula.Core.Elysium
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single, AddressFilterMode = AddressFilterMode.Any)]
    public class Master : IMasterServer
    {
        private ConcurrentDictionary<IPEndPoint, NodeReference> m_hKnownPeers;
        private ServiceHost                                     m_hHost;        

        public IPEndPoint LocalEndPoint { get; private set; }

        public event Action<NodeReference> ChannelClosed;
        public event Action<NodeReference> ChannelFaulted;

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

            (hNewNode as ICommunicationObject).Faulted += OnChannelFaulted;
            (hNewNode as ICommunicationObject).Closed  += OnChannelClosed;

            m_hKnownPeers.TryAdd(hNewNode.RemoteEndPoint, hNewNode);

        }

        private NodeReference HandleDisconnection(IMasterServerCallback hCb)
        {
            KeyValuePair<IPEndPoint, NodeReference> hRef = (from c in m_hKnownPeers where c.Value.Node == hCb select c).FirstOrDefault();
            NodeReference hRes;

            if (hRef.Key != null && m_hKnownPeers.TryRemove(hRef.Key, out hRes))
            {
                (hRef.Value as ICommunicationObject).Faulted -= OnChannelFaulted;
                (hRef.Value as ICommunicationObject).Closed -= OnChannelClosed;

                return hRes;
            }
            else
            {
                return null;
            }
        }

        private void OnChannelFaulted(object sender, EventArgs e)
        {
            IMasterServerCallback hCb = sender as IMasterServerCallback;
            NodeReference hRef = HandleDisconnection(hCb);
            ChannelFaulted?.Invoke(hRef);
        }

        private void OnChannelClosed(object sender, EventArgs e)
        {
            IMasterServerCallback hCb = sender as IMasterServerCallback;
            NodeReference hRef = HandleDisconnection(hCb);
            ChannelFaulted?.Invoke(hRef);
        }

        [ConsoleUIMethod]
        public IPEndPoint[] GetNodes(int iMaxNodes)
        {
            return m_hKnownPeers.Values.Take(iMaxNodes).Select(x => x.RemoteEndPoint).ToArray();
        }

        #region Nested Types
        public class NodeReference
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
