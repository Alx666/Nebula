using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Nebula.Core.Elysium
{
    [DataContract]
    public class NodeStatus
    {
        [DataMember]
        public int SomeData;
    }
}
