using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudioTools.Project;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace NPLTools.Project
{
    [Guid(Guids.NPLEditorFactoryGuidString)]
    public class NPLEditorFactory : CommonEditorFactory
    {
        public NPLEditorFactory(CommonProjectPackage package) : base(package) { }

        public NPLEditorFactory(CommonProjectPackage package, bool promptForEncoding) : base(package, promptForEncoding) { }
        protected override void InitializeLanguageService(IVsTextLines textLines)
        {
            InitializeLanguageService(textLines, typeof(NPLLanguageInfo).GUID);
        }

        public override int CreateEditorInstance(uint createEditorFlags, string documentMoniker, string physicalView, IVsHierarchy hierarchy, uint itemid, IntPtr docDataExisting, out IntPtr docView, out IntPtr docData, out string editorCaption, out Guid commandUIGuid, out int createDocumentWindowFlags)
        {
            var res = base.CreateEditorInstance(createEditorFlags, documentMoniker, physicalView, hierarchy, itemid, docDataExisting, out docView, out docData, out editorCaption, out commandUIGuid, out createDocumentWindowFlags);
            commandUIGuid = new Guid(Guids.NPLEditorFactoryGuidString);
            return res;
        }
    }
}
