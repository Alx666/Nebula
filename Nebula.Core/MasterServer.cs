using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.ServiceModel;

namespace Nebula.Core
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
    public class MasterServer : IMasterServer
    {
        private ServiceHost                 m_hHost;
        private ConcurrentBag<IPEndPoint>   m_hEndpoints; 

        public MasterServer()
        {
            m_hEndpoints = new ConcurrentBag<IPEndPoint>();
        }

        [ConsoleUIMethod]
        public void Start(int iPort)
        {
            m_hHost = new ServiceHost(this, new Uri($"net.tcp://localhost:{iPort}/NebulaMasterServer"));

            NetTcpBinding hBinding = new NetTcpBinding();
            hBinding.Security.Mode = SecurityMode.None;         
            m_hHost.AddServiceEndpoint(typeof(IMasterServer), hBinding, "");
            m_hHost.Open();
        }

        [ConsoleUIMethod]
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
            }
        }


        [ConsoleUIMethod]
        public IPEndPoint[] GetKnownNodes(int iAmmount)
        {
            return m_hEndpoints.Take(iAmmount).ToArray();
        }

        [ConsoleUIMethod]
        public int GetNodesCount()
        {
            return m_hEndpoints.Count;
        }

        public void Register()
        {
            m_hEndpoints.Add(OperationContext.Current.GetRemoteEndPoint());
        }
    }
}
