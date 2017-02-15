using Nebula.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Nebula.Modules.Repl
{
    public class ReplModule : INebulaModule
    {
        private const string InstallDirectory = ".\\Lib";
        public NebulaModuleInfo ModuleInfo  { get; private set; }

        private INebulaMasterService m_hOwner;
        private Dictionary<int, Process> m_hProcesses;


        public ReplModule()
        {
            ModuleInfo          = new NebulaModuleInfo("Repl", "6c6457c0-851b-499a-88f1-912a08952dc4", this);
            m_hProcesses        = new Dictionary<int, Process>();
        }

        public void Install()
        {
            Assembly hCurrent = this.GetType().Assembly;

            if (!Directory.Exists(InstallDirectory))
                Directory.CreateDirectory(InstallDirectory);

            List<string> hEmbeddedResources = hCurrent.GetManifestResourceNames().ToList();
            hEmbeddedResources.RemoveAt(0);

            foreach (var sName in hEmbeddedResources)
            {
                using (Stream hStream = this.GetType().Assembly.GetManifestResourceStream(sName))
                {
                    byte[] hAssemblyData = new byte[hStream.Length];
                    hStream.Read(hAssemblyData, 0, hAssemblyData.Length);

                    string sDllName = sName.Replace("Nebula.Modules.Repl.Lib.", "").Split(new char[] { ',' }).First();

                    File.WriteAllBytes(InstallDirectory + "\\" + sDllName, hAssemblyData);
                }
            }          
        }

        public void UnInstall()
        {
            throw new NotImplementedException();
        }

        public void Start(INebulaMasterService hOwner)
        {
            m_hOwner = hOwner;          
        }


        public void RegistrationComplete()
        {
        }
        

        private void ProcessOutputRedirect(object sender, DataReceivedEventArgs e)
        {
            m_hOwner.ModuleData(this.ModuleInfo.Guid, e.Data);
        }


        [NebulaModuleOperation]
        public int ReplStart()
        {
            Process hRepl                           = new Process();

            hRepl.StartInfo.FileName                = InstallDirectory + "\\REPL.exe";
            hRepl.StartInfo.RedirectStandardInput   = true;
            hRepl.StartInfo.RedirectStandardOutput  = true;
            hRepl.StartInfo.RedirectStandardError   = true;
            hRepl.StartInfo.UseShellExecute         = false;
            hRepl.EnableRaisingEvents               = true;
            hRepl.OutputDataReceived               += ProcessOutputRedirect;
            hRepl.Start();
            hRepl.BeginOutputReadLine();

            m_hProcesses.Add(hRepl.Id, hRepl);

            return hRepl.Id;
        }

        [NebulaModuleOperation]
        public void ReplCodeBlock(int iId, string sCode)
        {
            m_hProcesses[iId].StandardInput.WriteLine(sCode);
        }

        [NebulaModuleOperation]
        public void ReplStop(int iId)
        {
            m_hProcesses[iId].Close();
            m_hProcesses.Remove(iId);
        }

    }
}
