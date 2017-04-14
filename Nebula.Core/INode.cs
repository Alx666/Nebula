using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Nebula.Core
{
    [ServiceContract(CallbackContract = typeof(INodeCallback))]
    public interface INode
    {
        [OperationContract]
        IPEndPoint[] Join(int iMaxPeers);
        //[OperationContract(AsyncPattern = true)]
        //IAsyncResult BeginJoin(string sKnownAddress, int iPort, AsyncCallback hCb);

        //[OperationContract]
        //string[] EndJoin(IAsyncResult hRes);

  
    }
}
