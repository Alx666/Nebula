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

        public string SendQuery(string sString)
        {

            return sString;
        }

        [ConsoleUIMethod]
        public string SendQueryAtClient(int iIndex, string sString)
        {
            IBaseService hBase = m_hChannels.ToArray()[iIndex].Value;

            if (hBase is INode n)
                return n.SendQuery("Contract Call: " + sString);
            else
                return (hBase as INodeCallback).SendQuery("Callback Call: " + sString);            
        }
    }

}
