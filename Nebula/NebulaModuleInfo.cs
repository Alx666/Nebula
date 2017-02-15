using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Nebula.Shared
{
    [DataContract]
    public class NebulaModuleInfo
    {
        public NebulaModuleInfo()
        {
        }

        public NebulaModuleInfo(string sName, string sGuid, INebulaModule hModule)
        {
            Name = sName;
            Guid = new Guid(sGuid);

            Methods = (from m in hModule.GetType().GetMethods()
                       from a in m.GetCustomAttributes(true)
                       where a.GetType() == typeof(NebulaModuleOperationAttribute)
                       select new NebulaModuleMethod(m)).ToArray();
        }

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
        public NebulaModuleMethod()
        {

        }

        public NebulaModuleMethod(MethodInfo hMethodInfo)
        {
            Name        = hMethodInfo.Name;
            Parameters  = hMethodInfo.GetParameters().Select(m => new NebulaModuleMethodParameter(m.Name, m.ParameterType.Name)).ToArray();
            
            //IsPublic   = bIsPublic;
        }

        [DataMember]
        public string                           Name { get; set; }

        [DataMember]
        public NebulaModuleMethodParameter[]    Parameters { get; set; }
    }

    [DataContract]
    public class NebulaModuleMethodParameter
    {
        public NebulaModuleMethodParameter()
        {

        }

        public NebulaModuleMethodParameter(string sName, string sParameterType)
        {
            Name = sName;
            Type = sParameterType;
        }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Type { get; set; }
    }

}
