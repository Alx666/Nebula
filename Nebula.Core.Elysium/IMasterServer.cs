using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Nebula.Core;


namespace Nebula.Core.Elysium
{
    [ServiceContract(CallbackContract = typeof(INodeCallback))]
    public interface IMasterServer : IBaseService
    {

    }
}
