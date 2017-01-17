using System;
using System.ServiceModel;

namespace Nebula.Shared
{
    public interface INebulaMasterServiceCB
    {
        [OperationContract]
        string AddModule(byte[] hAssembly);

        [OperationContract]
        string RemoveModule(Guid vAssemblyId);        

        [OperationContract]
        NebulaModuleInfo[] ListModules();                  
    }
}
