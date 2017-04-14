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

namespace Nebula.Core
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
    public class Node : INode, IDisposable
    {
        public IPEndPoint LocalEndPoint { get; private set; }


        private ConcurrentDictionary<INode, IPEndPoint> m_hNodes;
        private ServiceHost m_hHost;
        private static int m_iCounter;

        public event Action<IPEndPoint> NodeFaulted;
        public event Action<IPEndPoint> NodeConnected;



        public Node()
        {
            m_hNodes = new ConcurrentDictionary<INode, IPEndPoint>();
        }

        public void Start(int iPort)
        {
            LocalEndPoint = new IPEndPoint(IPAddress.Loopback, iPort);

            m_hNodes.Clear();
            m_hHost = new ServiceHost(this, new Uri($"net.tcp://localhost:{iPort}/NebulaNode"));

            NetTcpBinding hBinding = new NetTcpBinding();
            hBinding.Security.Mode = SecurityMode.None;
            //hBinding.MaxBufferSize      = 51200;
            //hBinding.MaxBufferPoolSize  = 0;
            //hBinding.MaxReceivedMessageSize = 2147483647;

            m_hHost.AddServiceEndpoint(typeof(INode), hBinding, "");
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
                m_hNodes.Clear();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IPEndPoint[] EnumerateNodes()
        {
            return m_hNodes.Values.ToArray();
        }


        /// <summary>
        /// Connect the node to another node
        /// </summary>
        /// <param name="sKnowAddr">Ip address to the reachable Node</param>
        /// <param name="iPort">the port where the node instance is listening</param>
        public IPEndPoint[] Connect(string sKnowAddr, int iPort)
        {
            NetTcpBinding hBinding                  = new NetTcpBinding();
            hBinding.Security.Mode                  = SecurityMode.None;
            EndpointAddress hAddr                   = new EndpointAddress($"net.tcp://{sKnowAddr}:{iPort}/NebulaNode");
            DuplexChannelFactory<INode> hFactory    = new DuplexChannelFactory<INode>(typeof(INodeCallback), hBinding, hAddr);
            InstanceContext hContext                = new InstanceContext(this);
            INode hNetworkNode                      = hFactory.CreateChannel(hContext);

            if (!m_hNodes.TryAdd(hNetworkNode, new IPEndPoint(IPAddress.Parse(sKnowAddr), iPort)))
                throw new Exception("Could Not Connect to the Node");

            IPEndPoint[] hResult = hNetworkNode.Join(5);

            //TODO: deferred network connection, for now just connect to the other existing nodes
            foreach (IPEndPoint hRemoteEndPoint in hResult)
            {
                hNetworkNode = hFactory.CreateChannel(hContext, new EndpointAddress($"net.tcp://{hRemoteEndPoint.Address.ToString()}:{hRemoteEndPoint.Port}/NebulaNode"));
                hNetworkNode.Join(0);

                m_hNodes.TryAdd(hNetworkNode, new IPEndPoint(IPAddress.Parse(sKnowAddr), iPort));                    
            }

            return hResult;
        }

        

        public IPEndPoint[] Join(int iMaxPeers)
        {
            INode hCb               = OperationContext.Current.GetCallbackChannel<INode>();
            IPEndPoint[] hResult    = m_hNodes.Values.Take(iMaxPeers).ToArray();

            //(hCb as ICommunicationObject).Faulted += OnFaulted;

            IPEndPoint hRemoteEndPoint = OperationContext.Current.GetRemoteEndPoint();

            if (m_hNodes.TryAdd(hCb, hRemoteEndPoint))
            {
                NodeConnected?.Invoke(hRemoteEndPoint);
                return hResult;
            }
            else
                return null;
        }


        private void OnFaulted(object sender, EventArgs e)
        {
            INode hClient = sender as INode;
            IPEndPoint hRemoved;

            if (m_hNodes.TryRemove(hClient, out hRemoved))
            {
                NodeFaulted?.Invoke(hRemoved);
            }
            else
            {
                //TODO: accrocco temporaneo, prevedere sistema per la notifica dei  dei client mancanti
                throw new NotImplementedException();
            }
        }

        #region ContractOperations
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
