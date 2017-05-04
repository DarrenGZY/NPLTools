using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudioTools.Project;
using Microsoft.VisualStudio;
using System.Runtime.InteropServices;
using System.ComponentModel;
using Microsoft.VisualStudioTools;

namespace NPLTools.Project
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("F30D83A9-D13E-4FF2-A7BD-3685618EFA89")]
    public class NPLProjectNodeProperties : CommonProjectNodeProperties
    {
        internal NPLProjectNodeProperties(NPLProjectNode node) : base(node)
        {
        }

        [Browsable(false)]
        public VSLangProj.prjOutputType OutputTypeEx
        {
            get
            {
                return VSLangProj.prjOutputType.prjOutputTypeExe;
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        [Browsable(false)]
        public override VSLangProj.prjOutputType OutputType
        {
            get
            {
                // This is probably not entirely true, but it helps us deal with
                // extensions like Azure Tools that try to figure out whether we
                // support WebForms.
                return VSLangProj.prjOutputType.prjOutputTypeExe;
            }
            set { }
        }

        [Browsable(false)]
        public uint TargetFramework
        {
            get
            {
                // Cloud Service projects inspect this value to determine which
                // OS to deploy.
                switch (HierarchyNode.ProjectMgr.Site.GetUIThread().Invoke(() => Node.GetProjectProperty("TargetFrameworkVersion")))
                {
                    case "v4.0":
                        return 0x40000;
                    case "v4.5":
                        return 0x40005;
                    default:
                        return 0x40105;
                }
            }
        }
    }
}
