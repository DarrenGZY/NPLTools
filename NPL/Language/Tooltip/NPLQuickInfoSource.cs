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

namespace NPLTools.Language.Tooltip
{
    internal class NPLQuickInfoSource : IQuickInfoSource
    {
        private NPLQuickInfoSourceProvider _provider;
        private ITextBuffer _subjectBuffer;
        private Dictionary<string, string> _dictionary;
        private bool _isDisposed;

        public NPLQuickInfoSource(NPLQuickInfoSourceProvider provider, ITextBuffer subjectBuffer)
        {
            _provider = provider;
            _subjectBuffer = subjectBuffer;

            _dictionary = new Dictionary<string, string>();
            _dictionary.Add("add", "int add(int firstInt, int secondInt)\nAdds one integer to another.");
            _dictionary.Add("subtract", "int subtract(int firstInt, int secondInt)\nSubtracts one integer from another.");
            _dictionary.Add("multiply", "int multiply(int firstInt, int secondInt)\nMultiplies one integer by another.");
            _dictionary.Add("divide", "int divide(int firstInt, int secondInt)\nDivides one integer by another.");
        }

        public void AugmentQuickInfoSession(IQuickInfoSession session, IList<object> quickInfoContent, out ITrackingSpan applicableToSpan)
        {
            SnapshotPoint? subjectTriggerPoint = session.GetTriggerPoint(_subjectBuffer.CurrentSnapshot);
            if (!subjectTriggerPoint.HasValue)
            {
                applicableToSpan = null;
                return;
            }

            ITextSnapshot currentSnapshot = subjectTriggerPoint.Value.Snapshot;
            SnapshotSpan querySpan = new SnapshotSpan(subjectTriggerPoint.Value, 0);

            ITextStructureNavigator navigator = _provider.NavigatorService.GetTextStructureNavigator(_subjectBuffer);
            TextExtent extent = navigator.GetExtentOfWord(subjectTriggerPoint.Value);
            string searchText = extent.Span.GetText();

            foreach (string key in _dictionary.Keys)
            {
                int foundIndex = searchText.IndexOf(key, StringComparison.CurrentCultureIgnoreCase);
                if (foundIndex > -1)
                {
                    applicableToSpan = currentSnapshot.CreateTrackingSpan(extent.Span.Start + foundIndex, key.Length, SpanTrackingMode.EdgeInclusive);

                    string value;
                    _dictionary.TryGetValue(key, out value);
                    if (value != null)
                        quickInfoContent.Add(value);
                    else
                        quickInfoContent.Add("");

                    return;
                }
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

        public IQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {
            return new NPLQuickInfoSource(this, textBuffer);
        }
    }
}
