using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.ServiceModel;

namespace Nebula.Core
{
    public class Master : Service<IMasterServer, IMasterServerCallback>
    {
        private ConcurrentBag<IPEndPoint>   m_hEndpoints; 

        public Master(): base("NebulaMaster")
        {
            m_hEndpoints = new ConcurrentBag<IPEndPoint>();
        }

        [ConsoleUIMethod]
        public IPEndPoint[] GetKnownNodes(int iAmmount)
        {
            return m_hEndpoints.Take(iAmmount).ToArray();
        }




    }
}
