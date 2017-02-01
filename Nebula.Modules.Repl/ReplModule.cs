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
        public Exception LastError          { get; set; }
        public NebulaModuleInfo ModuleInfo  { get; private set; }

        private Process m_hRepl;
        private INebulaMasterService m_hOwner;

        public ReplModule()
        {
            ModuleInfo      = new NebulaModuleInfo();
            ModuleInfo.Methods = new NebulaModuleMethod[1] { new NebulaModuleMethod() };
            ModuleInfo.Methods[0].MethodName = "SendCodeBlock";
            ModuleInfo.Methods[0].Parameters = new string[] { "sCode" };
            ModuleInfo.Name = "Repl";
            ModuleInfo.Guid = Guid.NewGuid();
        }

        public void AssemblyInstalled(string sAssemblyFile, string sInstallFolder)
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

        public void Start(INebulaMasterService hOwner)
        {
            m_hOwner = hOwner;
            m_hRepl = new Process();
            m_hRepl.StartInfo.FileName = InstallDirectory + "\\REPL.exe";
            m_hRepl.StartInfo.RedirectStandardInput = true;
            m_hRepl.StartInfo.RedirectStandardOutput = true;
            m_hRepl.StartInfo.RedirectStandardError = true;
            m_hRepl.StartInfo.UseShellExecute = false;
            m_hRepl.EnableRaisingEvents = true;
            m_hRepl.OutputDataReceived += M_hRepl_OutputDataReceived;
            m_hRepl.Start();            
        }


        public void RegistrationComplete()
        {
            m_hRepl.BeginOutputReadLine();
        }
        

        private void M_hRepl_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            m_hOwner.ModuleData(this.ModuleInfo.Guid, e.Data);
        }

        private void OnProcessExited(object sender, EventArgs e)
        {
        }

        public string SendCodeBlock(string[] sCode)
        {
            m_hRepl.StandardInput.WriteLine(sCode.First());
            return "";
        }
    }
}
