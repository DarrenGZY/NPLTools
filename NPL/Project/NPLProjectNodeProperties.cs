using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudioTools.Project;
using Microsoft.VisualStudio;
using System.Runtime.InteropServices;
using System.ComponentModel;
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
    }
}
