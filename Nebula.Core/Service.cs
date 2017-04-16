using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Nebula.Core
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
    public abstract class Service<T>
    {
        public IPEndPoint LocalEndPoint { get; private set; }

        private ServiceHost m_hHost;
        private string      m_sUriAppend;

        public Service(string sUriAppenName)
        {
            m_sUriAppend = sUriAppenName;
        }


        [ConsoleUIMethod]
        public virtual void Start(int iPort)
        {
            m_hHost                 = new ServiceHost(this, new Uri($"net.tcp://localhost:{iPort}/{m_sUriAppend}"));
            LocalEndPoint           = new IPEndPoint(IPAddress.Loopback, iPort);
            NetTcpBinding hBinding  = new NetTcpBinding();
            hBinding.Security.Mode  = SecurityMode.None;

            m_hHost.AddServiceEndpoint(typeof(T), hBinding, "");
            m_hHost.Open();
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
    }
}
