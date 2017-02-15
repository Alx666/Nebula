using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nebula.Shared
{
    public interface INebulaModule
    {
        void Install();

        void UnInstall();

        void Start(INebulaMasterService hService);

        void RegistrationComplete();

        NebulaModuleInfo ModuleInfo { get; }
    }
}
