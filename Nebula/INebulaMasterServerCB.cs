using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Nebula.Shared
{
    public interface INebulaMasterServiceCB
    {
        [OperationContract]
        NebulaModuleInfo[] AddModule(byte[] hAssembly);

        [OperationContract]
        string RemoveModule(Guid vAssemblyId);        

        [OperationContract]
        NebulaModuleInfo[] ListModules();                  
    }
}
