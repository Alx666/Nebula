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
    public interface INode : IBaseService
    {
        [OperationContract]
        string SendQuery(string sString);
    }
}
