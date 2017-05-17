using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.OLE.Interop;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using System.Drawing;
using Microsoft.VisualStudioTools.Project;
using System.Windows.Forms;

namespace NPLTools.Project
{
    [Guid("BE2402BF-92AC-4467-9455-E9615D8F569F")]
    public class NPLPropertyPage : CommonPropertyPage
    {
        private readonly NPLPropertyPageControl _control;
        public NPLPropertyPage()
        {
            _control = new NPLPropertyPageControl(this);
        }

        public override Control Control
        {
            get
            {
                return _control;
            }
        }

        public override string Name
        {
            get
            {
                return "NPL Settings";
            }
        }

        public override void Apply()
        {
            Project.SetProjectProperty(NPLConstants.NPLExePath, _control.nplExePath);
            Project.SetProjectProperty(NPLConstants.NPLOptions, _control.nplExeOptions);
            Project.SetProjectProperty(NPLConstants.StartupFile, _control.scriptFile);
            Project.SetProjectProperty(NPLConstants.Arguments, _control.scriptArguments);
            Project.SetProjectProperty(NPLConstants.WorkingDirectory, _control.workingDir);
            IsDirty = false;
        }

        public override void LoadSettings()
        {
            Loading = true;
            try
            {
                _control.nplExePath = Project.GetUnevaluatedProperty(NPLConstants.NPLExePath);
                _control.nplExeOptions = Project.GetUnevaluatedProperty(NPLConstants.NPLOptions);
                _control.scriptFile = Project.GetUnevaluatedProperty(NPLConstants.StartupFile);
                _control.scriptArguments = Project.GetUnevaluatedProperty(NPLConstants.Arguments);
                _control.workingDir = Project.GetUnevaluatedProperty(NPLConstants.WorkingDirectory);
            }finally
            {
                Loading = false;
            }
            
        }
    }
}
