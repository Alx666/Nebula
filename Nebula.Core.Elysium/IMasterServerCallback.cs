using System.ServiceModel;

namespace Nebula.Core.Elysium
{
    public interface IMasterServerCallback : IBaseService
    {
        [OperationContract]
        NodeStatus GetNodeStatus();
    }
}
