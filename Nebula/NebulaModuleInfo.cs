using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Nebula.Shared
{
    [DataContract]
    public class NebulaModuleInfo
    {
        [DataMember]
        public string   Name    { get; set; }

        [DataMember]
        public Guid     Guid    { get; set; }
        
        [DataMember]
        public NebulaModuleMethod[] Methods {get; set;}

        public override string ToString() => Name;
    }

    [DataContract]
    public class NebulaModuleMethod
    {
        [DataMember]
        public string   MethodName;

        [DataMember]
        public string[] Parameters;
    }

}
