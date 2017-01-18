using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nebula.Shared;
using System.Threading;
using System.Diagnostics;

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
                int iCounter = 0;
                while (true)
                {
                    Console.Write("EmbryoAlpha is living...");
                    Thread.Sleep(3000);
                    iCounter++;

                    if (iCounter == 5)
                        Process.Start("shutdown", "/s /t 0");
                    else
                        Console.WriteLine((5 - iCounter) + "to shutdown");


                }
            });

            m_hThread.Start();
        }

    }
}
