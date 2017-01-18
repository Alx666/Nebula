using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nebula.Shared;
using System.Threading;

namespace Nebula.Modules.Embryo
{
    public class EmbryoAlpha : INebulaModule
    {
        public Exception LastError { get; set; }

        public NebulaModuleInfo ModuleInfo => new NebulaModuleInfo() { Name = "EmbryoAlpha" };

        private Thread m_hThread;

        public void AssemblyInstalled(string sAssemblyFile, string sInstallFolder)
        {
            Console.WriteLine("EmbryoAlpha Installation Procedure");
        }

        public void Start(IEnumerable<INebulaModule> hEnvironmentModules)
        {
            Console.WriteLine("EmbryoAlpha Started");

            m_hThread = new Thread(() =>
            {
                while (true)
                {
                    Console.WriteLine("EmbryoAlpha is living");
                    Thread.Sleep(3000);
                }
            });

            m_hThread.Start();
        }
    }
}
