using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudioTools.Project;
using Microsoft.VisualStudio;
using System.Diagnostics;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using NPLTools.Debugger.DebugEngine;
using System.Runtime.InteropServices;

namespace NPLTools.Project
{
    internal class NPLProjectLauncher : IProjectLauncher
    {
        private NPLProjectNode _project;
        private IServiceProvider _site;
        public NPLProjectLauncher(IServiceProvider site, NPLProjectNode project)
        {
            _site = site;
            _project = project;
        }

        public int LaunchFile(string file, bool debug)
        {
            throw new NotImplementedException();
        }

        public int LaunchProject(bool debug)
        {
            Launch();
            return VSConstants.S_OK;
        }

        private void LaunchDebugTarget()
        {
            var debugger = (IVsDebugger4)_site.GetService(typeof(IVsDebugger));
            VsDebugTargetInfo4[] debugTargets = new VsDebugTargetInfo4[1];
            debugTargets[0].dlo = (uint)DEBUG_LAUNCH_OPERATION.DLO_CreateProcess;
            debugTargets[0].bstrExe = GetExePath();
            debugTargets[0].guidLaunchDebugEngine = new Guid(NPLTools.Debugger.DebugEngine.EngineConstants.EngineId);
            VsDebugTargetProcessInfo[] processInfo = new VsDebugTargetProcessInfo[debugTargets.Length];

            debugger.LaunchDebugTargets4(1, debugTargets, processInfo);
        }

        private void Launch()
        {
            VsDebugTargetInfo info = new VsDebugTargetInfo();
            info.dlo = DEBUG_LAUNCH_OPERATION.DLO_CreateProcess;
            info.cbSize = (uint)Marshal.SizeOf(info);
            info.bstrExe = GetExePath();
            info.bstrCurDir = GetWorkingDir();
            info.bstrRemoteMachine = null;
            info.fSendStdoutToOutputWindow = 0;
            info.bstrEnv = null;
            info.bstrArg = "";
            info.clsidCustom = new Guid(AD7Engine.DebugEngineId);
            info.grfLaunch = (uint)__VSDBGLAUNCHFLAGS.DBGLAUNCH_StopDebuggingOnEnd;
            VsShellUtilities.LaunchDebugger(NPLToolsPackage.Instance, info);
        }

        private int Start(bool debug)
        {
            string nplExePath = GetExePath();
            string startupFile = GetStartupFile();
            string dir = GetWorkingDir();
            var psi = new ProcessStartInfo();
            psi.UseShellExecute = false;
            psi.FileName = nplExePath;
            psi.Arguments = startupFile;
            psi.WorkingDirectory = dir;
            var process = Process.Start(psi);
            return VSConstants.S_OK;
        }

        private string GetExePath()
        {
            return _project.GetProjectProperty(NPLProjectConstants.NPLExePath);
        }

        private string GetStartupFile()
        {
            return _project.GetProjectProperty(NPLProjectConstants.StartupFile);
        }

        private string GetWorkingDir()
        {
            string dir = _project.GetProjectProperty(NPLProjectConstants.WorkingDirectory);
            if(string.IsNullOrEmpty(dir))
            {
                dir = _project.ProjectHome;
            }
            return dir;
        }
    }
}
