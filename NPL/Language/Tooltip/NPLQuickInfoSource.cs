using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.TextManager.Interop;
using NPLTools.IronyParser.Ast;
using Irony.Parsing;
using NPLTools.IronyParser;
using NPLTools.Language;
using Microsoft.VisualStudio.Shell;
using NPLTools.Intellisense;

namespace NPLTools.Language.Tooltip
{
    internal class NPLQuickInfoSource : IQuickInfoSource
    {
        private NPLQuickInfoSourceProvider _provider;
        private ITextBuffer _subjectBuffer;
        private AnalysisEntry _analysisEntry;
        private bool _isDisposed;

        public NPLQuickInfoSource(NPLQuickInfoSourceProvider provider, ITextBuffer subjectBuffer)
        {
            _provider = provider;
            _subjectBuffer = subjectBuffer;
            _analysisEntry = AnalysisEntryInitializer.Initialize(provider.ServiceProvider, subjectBuffer);
        }

        public void AugmentQuickInfoSession(IQuickInfoSession session, IList<object> quickInfoContent, out ITrackingSpan applicableToSpan)
        {
            SnapshotPoint? subjectTriggerPoint = session.GetTriggerPoint(_subjectBuffer.CurrentSnapshot);
            if (!subjectTriggerPoint.HasValue)
            {
                applicableToSpan = null;
                return;
            }

            //ITextSnapshot currentSnapshot = subjectTriggerPoint.Value.Snapshot;
            //SnapshotSpan querySpan = new SnapshotSpan(subjectTriggerPoint.Value, 0);

            ITextStructureNavigator navigator = _provider.NavigatorService.GetTextStructureNavigator(_subjectBuffer);
            TextExtent extent = navigator.GetExtentOfWord(subjectTriggerPoint.Value);
            //string searchText = extent.Span.GetText();

            string description = _analysisEntry.GetDescription(_subjectBuffer, subjectTriggerPoint.Value);

            if (description != String.Empty && description != null)
            {
                applicableToSpan = subjectTriggerPoint.Value.Snapshot.CreateTrackingSpan(extent.Span.Start, extent.Span.Length, SpanTrackingMode.EdgeInclusive);
                quickInfoContent.Add(description);
                return;
            }

            applicableToSpan = null;
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                GC.SuppressFinalize(this);
                _isDisposed = true;
            }
        }
    }

    [Export(typeof(IQuickInfoSourceProvider))]
    [Name("ToolTip QuickInfo Source")]
    [Order(Before = "Default Quick Info Presenter")]
    [ContentType("NPL")]
    internal class NPLQuickInfoSourceProvider : IQuickInfoSourceProvider
    {
        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        [Import]
        internal ITextBufferFactoryService TextBufferFactoryService { get; set; }

        [Import]
        internal SVsServiceProvider ServiceProvider { get; private set; }

        public IQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {
            return new NPLQuickInfoSource(this, textBuffer);
        }
    }
}
