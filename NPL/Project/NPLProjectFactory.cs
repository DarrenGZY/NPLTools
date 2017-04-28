using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudioTools.Project;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace NPLTools.Project
{
    [Guid(Guids.NPLProjectFactoryGuidString)]
    class NPLProjectFactory : ProjectFactory
    {

        public NPLProjectFactory(IServiceProvider package)
            : base(package)
        {
        }

        internal override ProjectNode CreateProject()
        {
            // Ensure our package is properly loaded
            // var pyService = Site.GetPythonToolsService();

            return new NPLProjectNode(Site);
        }
    }
}
