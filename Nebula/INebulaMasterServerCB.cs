using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Nebula.Shared
{
    public interface INebulaMasterServiceCB
    {
        [OperationContract]
        NebulaModuleInfo[] Add(byte[] hAssembly);

        [OperationContract]
        void Remove(Guid vAssemblyId);

        [OperationContract]
        NebulaModuleInfo[] GetModules();

        [OperationContract]
        string Execute(Guid vId, string sMethodName, string[] hParams);
    }
}
