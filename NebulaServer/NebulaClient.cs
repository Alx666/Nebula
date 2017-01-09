using Nebula.Shared;

namespace Nebula.Server
{
    public class NebulaClient
    {
        public NebulaClient(int iId, INebulaMasterServiceCB hCb)
        {
            Id = iId;
            Callback = hCb;
        }
        public int Id { get; set; }
        public INebulaMasterServiceCB Callback { get; set; }
    }
}
