using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nebula.Shared
{
    public interface INebulaModule
    {
        Exception LastError { get; set; }

        void AssemblyInstalled(string sAssemblyFile, string sInstallFolder);

        void Start(IEnumerable<INebulaModule> hEnvironmentModules);

        NebulaModuleInfo ModuleInfo { get; }
    }
}
