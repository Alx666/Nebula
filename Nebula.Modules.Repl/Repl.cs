using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nebula.Shared;
using Microsoft.CodeAnalysis.Scripting;
using System.IO;
using System.Reflection;

namespace Nebula.Modules.Repl
{
    public class Repl : INebulaModule
    {
        private ScriptState<object> m_hScriptState;
        private ScriptOptions       m_hOptions;

        public Exception LastError { get; set; }

        public NebulaModuleInfo ModuleInfo { get; private set; }


        public Repl()
        {
            ModuleInfo = new NebulaModuleInfo();
            ModuleInfo.Name = "Repl";
            ModuleInfo.Guid = Guid.NewGuid();
        }

        public void AssemblyInstalled(string sAssemblyFile, string sInstallFolder)
        {
        }

        public void Start(IEnumerable<INebulaModule> hEnvironmentModules)
        {
            Assembly hCurrent = this.GetType().Assembly;

            string[] hResources = hCurrent.GetManifestResourceNames();

            for (int i = 0; i < hResources.Length; i++)
            {
                using (Stream hStream = this.GetType().Assembly.GetManifestResourceStream(hResources[i]))
                {
                    byte[] hAssemblyData = new byte[hStream.Length];
                    hStream.Read(hAssemblyData, 0, hAssemblyData.Length);
                    Assembly.Load(hAssemblyData);
                }
            }

            m_hOptions = m_hOptions.AddReferences(typeof(System.Object).Assembly, typeof(System.Linq.Enumerable).Assembly);
            m_hOptions = m_hOptions.AddImports("System");
            m_hOptions = m_hOptions.AddImports("System.Linq");
            m_hOptions = m_hOptions.AddImports("System.Collections.Generic");
        }
    }
}
