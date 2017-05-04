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
using NPLTools.Intellisense;
using Microsoft.VisualStudio.Shell;
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

        private ITextBuffer _textBuffer;
        private AnalysisEntry _analysisEntry;

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            ITextView textView = AdaptersFactory.GetWpfTextView(textViewAdapter);
            _textBuffer = textView.TextBuffer;
            //NPLProjectNode project = ServiceProvider.GetService(typeof(CommonProjectNode)) as NPLProjectNode;
            //IServiceProvider serviceProvider = ServiceProvider as IServiceProvider;
            //IVsSolution sln = serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
            //NPLProjectNode project = sln.GetLoadedProject().GetNPLProject();
            //if (!project.GetAnalyzer().HasMonitoredTextBuffer(textView.TextBuffer))
            //    project.GetAnalyzer().MonitorTextBuffer(textView.TextBuffer);

            //_analysisEntry = textView.TextBuffer.GetAnalysisAtCaret(ServiceProvider);
            //textView.Closed += TextView_Closed;
            IOleCommandTarget next;
            NPLEditorCommandFilter commandFilter = new NPLEditorCommandFilter(textView, textViewAdapter, ServiceProvider);
            textViewAdapter.AddCommandFilter(commandFilter, out next);
            commandFilter.Next = next;
        }

        //private void TextView_Closed(object sender, EventArgs e)
        //{
        //    close();
        //}

        //private void close()
        //{
        //    IServiceProvider serviceProvider = ServiceProvider as IServiceProvider;
        //    IVsSolution sln = serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
        //    NPLProjectNode project = sln.GetLoadedProject().GetNPLProject();
        //    //if (!project.GetAnalyzer().HasMonitoredTextBuffer(_textBuffer))
        //    project.GetAnalyzer().CanceledMonitorTextBuffer(_analysisEntry, _textBuffer);
        //}
    }
}
