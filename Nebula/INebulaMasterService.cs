using System.ServiceModel;

namespace Nebula.Shared
{
    [ServiceContract(CallbackContract = typeof(INebulaMasterServiceCB))]
    public interface INebulaMasterService
    {
        [OperationContract]
        string Execute(int iClientId, string sBinary);

        [OperationContract]
        void Register(string sMachineInfo);
    }
}
