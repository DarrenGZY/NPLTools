using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudioTools.Project;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio;

namespace NPLTools.Language
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
    }
}
