using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Nebula.Core
{
    [ServiceContract]
    public interface IMasterServer
    {
        [OperationContract(IsOneWay = true)]
        void Register();

        [OperationContract]
        IPEndPoint[] GetKnownNodes(int iAmmount);

        [OperationContract]
        int GetNodesCount();
    }
}
