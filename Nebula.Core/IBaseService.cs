using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.ServiceModel;

namespace Nebula.Core
{
    [ServiceContract]
    public interface IBaseService
    {
        [OperationContract]
        IPEndPoint[] Notify(int iListenPort);

        [OperationContract]
        int GetNodesCount();

        [OperationContract]
        IPEndPoint[] GetNodes(int iMaxNodes, IPEndPoint[] hExcludes);
    }
}
