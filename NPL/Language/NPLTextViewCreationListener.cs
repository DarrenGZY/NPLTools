using System;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text;
using IOleCommandTarget = Microsoft.VisualStudio.OLE.Interop.IOleCommandTarget;
using System.Runtime.InteropServices;
using Irony.Parsing;
using Irony.Interpreter.Ast;
using NPLTools.IronyParser.Ast;
using NPLTools.IronyParser;
using NPLTools.Intelligense;
using Microsoft.VisualStudio.Shell;
using NPLTools.Intelligense;
using NPLTools.Project;
using Microsoft.VisualStudioTools.Project;
using Microsoft.VisualStudio.Shell.Interop;

namespace NPLTools.Language
{
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType("NPL")]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    class NPLTextViewCreationListener : IVsTextViewCreationListener
    {
        [Import]
        public IVsEditorAdaptersFactoryService AdaptersFactory { get; private set; }

        [Import]
        public SVsServiceProvider ServiceProvider { get; private set; } 

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            ITextView textView = AdaptersFactory.GetWpfTextView(textViewAdapter);
            //NPLProjectNode project = ServiceProvider.GetService(typeof(CommonProjectNode)) as NPLProjectNode;
            IServiceProvider serviceProvider = ServiceProvider as IServiceProvider;
            IVsSolution sln = serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
            NPLProjectNode project = sln.GetLoadedProject().GetNPLProject();
            project.GetAnalyzer().MonitorTextBuffer(textView.TextBuffer);
            IOleCommandTarget next;
            NPLEditorCommandFilter commandFilter = new NPLEditorCommandFilter(textView, textViewAdapter, ServiceProvider);
            textViewAdapter.AddCommandFilter(commandFilter, out next);
            commandFilter.Next = next;
        }
    }
}
