using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using NPLTools.Intelligense;

namespace NPLTools.Language.AutoCompletion
{
    internal class NPLCompletionSource : ICompletionSource
    {
        private NPLCompletionSourceProvider _sourceProvider;
        private ITextBuffer _textBuffer;
        private List<Completion> _compList;

        public NPLCompletionSource(NPLCompletionSourceProvider sourceProvider, ITextBuffer textBuffer)
        {
            _sourceProvider = sourceProvider;
            _textBuffer = textBuffer;
        }

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            //ITrackingPoint point = session.GetTriggerPoint(_textBuffer);
            SnapshotPoint? triggerPoint = session.GetTriggerPoint(_textBuffer.CurrentSnapshot);
            
            //point.Value.Snapshot.
            //_textBuffer.

            List<string> strList = new List<string>();
            if (LuaModel.Declarations != null && triggerPoint.HasValue)
            {
                foreach (var keyValue in LuaModel.Declarations)
                {
                    if (LuaModel.IsInScope(triggerPoint.Value.Position, keyValue.Value))
                        strList.Add(keyValue.Key);
                }
            }

            //strList.Add("addtion");
            //strList.Add("adaptation");
            //strList.Add("subtraction");
            //strList.Add("summation");
            _compList = new List<Completion>();
            foreach (string str in strList)
                _compList.Add(new Completion(str, str, str, null, null));

            completionSets.Add(new CompletionSet(
                "Tokens",
                "Tokens",
                FindTokenSpanAtPosition(session.GetTriggerPoint(_textBuffer),
                    session),
                _compList,
                null));
        }

        private ITrackingSpan FindTokenSpanAtPosition(ITrackingPoint point, ICompletionSession session)
        {
            SnapshotPoint currentPoint = (session.TextView.Caret.Position.BufferPosition) - 1;
            ITextStructureNavigator navigator = _sourceProvider.NavigatorService.GetTextStructureNavigator(_textBuffer);
            TextExtent extent = navigator.GetExtentOfWord(currentPoint);
            return currentPoint.Snapshot.CreateTrackingSpan(extent.Span, SpanTrackingMode.EdgeInclusive);
        }

        private bool _isDisposed;

        public void Dispose()
        {
            if (!_isDisposed)
            {
                GC.SuppressFinalize(this);
                _isDisposed = true;
            }
        }
    }

    [Export(typeof(ICompletionSourceProvider))]
    [ContentType("NPL")]
    [Name("tokien completion")]
    internal class NPLCompletionSourceProvider : ICompletionSourceProvider
    {
        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }


        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            return new NPLCompletionSource(this, textBuffer);
        }
    }
}
