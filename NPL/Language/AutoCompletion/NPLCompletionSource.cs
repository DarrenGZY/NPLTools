﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using NPLTools.Intellisense;
using Microsoft.VisualStudio.Shell;
using NPLTools.IronyParser.Ast;

namespace NPLTools.Language.AutoCompletion
{
    internal class NPLCompletionSource : ICompletionSource
    {
        private NPLCompletionSourceProvider _provider;
        private ITextBuffer _textBuffer;
        private List<Completion> _compList;
        private AnalysisEntry _analysisEntry;

        public NPLCompletionSource(NPLCompletionSourceProvider provider, ITextBuffer textBuffer)
        {
            _provider = provider;
            _textBuffer = textBuffer;
            _analysisEntry = AnalysisEntryInitializer.Initialize(provider.ServiceProvider, textBuffer);
        }

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            //ITrackingPoint point = session.GetTriggerPoint(_textBuffer);
            SnapshotPoint? triggerPoint = session.GetTriggerPoint(_textBuffer.CurrentSnapshot);

            if (!triggerPoint.HasValue) return;
            List<Declaration> strList = _analysisEntry.GetCompletionSource(triggerPoint.Value.Position).ToList();

            strList = strList.OrderBy(i => i.Name).ToList();

            _compList = new List<Completion>();
            foreach (var str in strList)
                _compList.Add(new Completion(str.Name, str.Name, str.Name, _provider.GetImageSource(str.Type), null));

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
            ITextStructureNavigator navigator = _provider.NavigatorService.GetTextStructureNavigator(_textBuffer);
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

        [Import]
        internal SVsServiceProvider ServiceProvider { get; private set; }

        [Import]
        IGlyphService GlyphService { get; set; }

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            return new NPLCompletionSource(this, textBuffer);
        }

        public System.Windows.Media.ImageSource GetImageSource(LuaDeclarationType type)
        {
            StandardGlyphGroup group;
            StandardGlyphItem item = StandardGlyphItem.GlyphItemPublic;
            switch (type)
            {
                case LuaDeclarationType.Table: group = StandardGlyphGroup.GlyphGroupClass; break;
                case LuaDeclarationType.Function: group = StandardGlyphGroup.GlyphGroupMethod; break;
                case LuaDeclarationType.Unknown: group = StandardGlyphGroup.GlyphGroupField; break;
                default:
                    group = StandardGlyphGroup.GlyphGroupVariable; break;
            }
            return GlyphService.GetGlyph(group, item);
        }
    }
}
