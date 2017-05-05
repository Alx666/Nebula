using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Nebula.Core;

namespace Nebula.Core.Elysium
{    
    public interface INodeCallback : IBaseService
    {
        [OperationContract]
        List<string> Query(string sString);

        [OperationContract(IsOneWay = true)]
        void Store(string sString);
    }
}
