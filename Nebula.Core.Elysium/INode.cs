using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Nebula.Core;

namespace Nebula.Core.Elysium
{
    [ServiceContract(CallbackContract = typeof(INodeCallback))]
    public interface INode : IBaseService
    {
        [OperationContract]
        List<string> Query(string sString);

        [OperationContract(IsOneWay = true)]
        void Store(string sString);
    }
}
