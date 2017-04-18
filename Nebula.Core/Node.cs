using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nebula.Core
{
    public class Node : Service<INode, INodeCallback>, INode, INodeCallback
    {
        public Node() : base("NebulaNode")
        {
        }

        public void CustomCallbackLogic()
        {
            throw new NotImplementedException();
        }

        public void CustomServiceLogic()
        {
            throw new NotImplementedException();
        }
    }

}
