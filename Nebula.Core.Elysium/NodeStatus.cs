using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Net;

namespace Nebula.Core.Elysium
{
    [DataContract]
    public class NodeStatus
    {
        [DataMember]
        public string   Address;

        [DataMember]
        public int      Port;

    }
}
