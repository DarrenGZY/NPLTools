using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using NPLTools.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLTools.Intellisense
{
    public static class AnalysisEntryInitializer
    {
        public static AnalysisEntry Initialize(IServiceProvider serviceProvider, ITextBuffer textBuffer)
        {
            AnalysisEntry analysisEntry;
            IVsSolution sln = serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
            IVsProject proj = sln.GetLoadedProject();
            // if not a NPL project
            if (proj == null)
            {
                analysisEntry = textBuffer.GetAnalysisAtCaretSingletonMode();
                // if not register yet, register it
                if (analysisEntry == null)
                {
                    analysisEntry = new AnalysisEntry(textBuffer.GetFilePath());
                    analysisEntry.RegisterSingletonMode(textBuffer);
                }
            }
            else
            {
                var project = sln.GetLoadedProject().GetNPLProject();
                if (!project.Analyzer.HasMonitoredTextBuffer(textBuffer))
                    project.Analyzer.MonitorTextBuffer(textBuffer);

                analysisEntry = textBuffer.GetAnalysisAtCaretProjectMode(serviceProvider);
            }
            return analysisEntry;
        }
    }
}
