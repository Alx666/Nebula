using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Nebula.Core
{
    [ServiceContract(CallbackContract = typeof(INodeCallback))]
    public interface IMasterServer : IBaseService
    {

    }
}
