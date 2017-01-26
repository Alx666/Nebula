using System;
using System.ServiceModel;

namespace Nebula.Shared
{
    [ServiceContract(CallbackContract = typeof(INebulaMasterServiceCB))]
    public interface INebulaMasterService
    {
        [OperationContract]
        void Register(string sMachineInfo, NebulaModuleInfo[] hModules);

        [OperationContract]
        void ModuleData(Guid vModuleId, string sData);
    }
}
