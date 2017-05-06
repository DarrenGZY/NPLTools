using Irony.Interpreter.Ast;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudioTools;
using NPLTools.Intellisense;
using NPLTools.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLTools.Language
{
    internal static class NPLExtensions
    {
        internal static IComponentModel GetComponentModel(this IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                return null;
            }
            return (IComponentModel)serviceProvider.GetService(typeof(SComponentModel));
        }

        internal static AnalysisEntryService GetEntryService(this IServiceProvider serviceProvider)
        {
            return serviceProvider.GetComponentModel()?.GetService<AnalysisEntryService>();
        }

        internal static AnalysisEntry GetAnalysisAtCaretProjectMode(this ITextView textView, IServiceProvider serviceProvider)
        {
            var service = serviceProvider.GetEntryService();
            AnalysisEntry entry = null;
            service?.TryGetAnalysisEntry(textView, textView.TextBuffer, out entry);
            return entry;
        }

        internal static AnalysisEntry GetAnalysisAtCaretProjectMode(this ITextBuffer textBuffer, IServiceProvider serviceProvider)
        {
            var service = serviceProvider.GetEntryService();
            AnalysisEntry entry = null;
            service?.TryGetAnalysisEntry(textBuffer, out entry);
            return entry;
        }

        internal static AnalysisEntry GetAnalysisAtCaretSingletonMode(this ITextView textView)
        {
            AnalysisEntry entry = null;
            textView.TextBuffer.Properties.TryGetProperty(typeof(AnalysisEntry), out entry);
            return entry;
        }

        internal static AnalysisEntry GetAnalysisAtCaretSingletonMode(this ITextBuffer textBuffer)
        {
            AnalysisEntry entry = null;
            textBuffer.Properties.TryGetProperty(typeof(AnalysisEntry), out entry);
            return entry;
        }

        public static string GetFilePath(this ITextBuffer textBuffer)
        {
            ITextDocument textDocument;
            if (textBuffer.Properties.TryGetProperty(typeof(ITextDocument), out textDocument))
            {
                return textDocument.FilePath;
            }
            return null;
        }

        public static IVsProject GetLoadedProject(this IVsSolution solution)
        {
            var guid = new Guid(NPLTools.Project.Guids.NPLProjectFactoryGuidString);
            IEnumHierarchies hierarchies;
            ErrorHandler.ThrowOnFailure((solution.GetProjectEnum(
                (uint)(__VSENUMPROJFLAGS.EPF_MATCHTYPE | __VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION),
                ref guid,
                out hierarchies)));
            IVsHierarchy[] hierarchy = new IVsHierarchy[1];
            uint fetched;
            if (ErrorHandler.Succeeded(hierarchies.Next(1, hierarchy, out fetched)) && fetched == 1)
            {
                var project = hierarchy[0] as IVsProject;
                if (project != null)
                {
                    return project;
                }
            }
            return null;
        }

        internal static NPLProjectNode GetNPLProject(this IVsProject project)
        {
            return ((IVsHierarchy)project).GetProject().GetCommonProject() as NPLProjectNode;
        }
    }
}
