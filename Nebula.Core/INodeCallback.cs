using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Nebula.Core
{
    
    public interface INodeCallback
    {
        [OperationContract]
        void PlaceHolder();
    }
}
