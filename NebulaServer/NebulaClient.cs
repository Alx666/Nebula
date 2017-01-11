using Nebula.Shared;
using System.Net;

namespace Nebula.Server
{
    public class NebulaClient
    {
        public NebulaClient(int iId, INebulaMasterServiceCB hCb, string sMachineName, IPEndPoint hRemoteAddr)
        {
            Id          = iId;
            Callback    = hCb;
            Machine     = sMachineName;
            Address     = hRemoteAddr;
        }

        public int          Id          { get; set; }
        public string       Machine     { get; private set; }
        public IPEndPoint   Address     { get; private set; }

        internal INebulaMasterServiceCB Callback { get; set; }
    }
}
