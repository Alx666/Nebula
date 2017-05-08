using System.ServiceModel;

namespace Nebula.Core.Elysium
{
    public interface IMasterServerCallback
    {
        [OperationContract]
        NodeStatus GetNodeStatus();
    }
}
