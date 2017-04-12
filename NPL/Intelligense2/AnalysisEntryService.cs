using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Differencing;
using Microsoft.VisualStudio.Text.Editor;
using NPLTools.Language;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using Task = System.Threading.Tasks.Task;
using System.Threading.Tasks;

namespace NPLTools.Intelligense2
{
    public interface IAnalysisEntryService
    {
        bool TryGetAnalyzer(ITextBuffer textBuffer, out ProjectAnalyzer analyzer, out string filename);
        bool TryGetAnalyzer(ITextView textView, out ProjectAnalyzer analyzer, out string filename);

        Task WaitForAnalyzerAsync(ITextBuffer textBuffer, CancellationToken cancellationToken);
        Task WaitForAnalyzerAsync(ITextView textView, CancellationToken cancellationToken);

        ProjectAnalyzer DefaultAnalyzer { get; }
    }

    [Export(typeof(IAnalysisEntryService))]
    [Export(typeof(AnalysisEntryService))]
    class AnalysisEntryService : IAnalysisEntryService
    {
        private readonly IServiceProvider _site;
        private readonly IComponentModel _model;
        private readonly IWpfDifferenceViewerFactoryService _diffService;

        private static readonly object _waitForAnalyzerKey = new object();

        public ProjectAnalyzer DefaultAnalyzer
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        [ImportingConstructor]
        public AnalysisEntryService([Import(typeof(SVsServiceProvider))] IServiceProvider site)
        {
            _site = site;
            _model = _site.GetComponentModel();

            try
            {
                _diffService = _model.GetService<IWpfDifferenceViewerFactoryService>();
            }
            catch (CompositionException)
            {
            }
            catch (ImportCardinalityMismatchException)
            {
            }
        }

        public bool TryGetAnalysisEntry(ITextView textView, ITextBuffer textBuffer, out AnalysisEntry entry)
        {
            ProjectAnalyzer analyzer;
            string filename;
            if ((textView == null || !TryGetAnalyzer(textView, out analyzer, out filename)) &&
                !TryGetAnalyzer(textBuffer, out analyzer, out filename))
            {
                entry = null;
                return false;
            }

            if (analyzer != null)
            {
                entry = analyzer.GetAnalysisEntryFromPath(filename);
                return entry != null;
            }

            entry = null;
            return true;
        }

        public void SetAnalyzer(ITextBuffer textBuffer, ProjectAnalyzer analyzer)
        {
            if (textBuffer == null)
            {
                throw new ArgumentNullException(nameof(textBuffer));
            }

            if (analyzer == null)
            {
                textBuffer.Properties.RemoveProperty(typeof(ProjectAnalyzer));
                return;
            }

            textBuffer.Properties[typeof(ProjectAnalyzer)] = analyzer;
        }

        public bool TryGetAnalyzer(ITextBuffer textBuffer, out ProjectAnalyzer analyzer, out string filename)
        {
            if (textBuffer == null)
            {
                throw new ArgumentNullException(nameof(textBuffer));
            }

            // If we have set an analyzer explicitly, return that
            analyzer = null;
            if (textBuffer.Properties.TryGetProperty(typeof(ProjectAnalyzer), out analyzer))
            {
                filename = textBuffer.GetFilePath();
                return analyzer != null;
            }

            analyzer = null;
            filename = null;
            return false;
        }

        public bool TryGetAnalyzer(ITextView textView, out ProjectAnalyzer analyzer, out string filename)
        {
            if (textView == null)
            {
                throw new ArgumentNullException(nameof(textView));
            }

            // Try to get the analyzer from the main text buffer
            if (TryGetAnalyzer(textView.TextBuffer, out analyzer, out filename))
            {
                return true;
            }

            analyzer = null;
            filename = null;
            return false;
        }

        public Task WaitForAnalyzerAsync(ITextBuffer textBuffer, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task WaitForAnalyzerAsync(ITextView textView, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
