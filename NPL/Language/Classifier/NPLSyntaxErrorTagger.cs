using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Irony;
using Irony.Parsing;
using NPLTools.IronyParser;
using NPLTools.Intelligense;
using Microsoft.VisualStudio.Shell;

namespace NPLTools.Language.Classifier
{
    [Export(typeof(ITaggerProvider))]
    [ContentType("NPL")]
    [TagType(typeof(ErrorTag))]
    internal sealed class NPLSyntaxErrorTaggerProvider : ITaggerProvider
    {
        [Import]
        internal SVsServiceProvider ServiceProvider { get; private set; }

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            return new NPLSyntaxErrorTagger(this, buffer) as ITagger<T>;
        }
    }

    internal sealed class NPLSyntaxErrorTagger : ITagger<ErrorTag>
    {
        private ITextBuffer _buffer;
        private List<ITagSpan<ErrorTag>> _syntaxErrorTags;
        private LogMessageList _syntaxErrorMessages;
        public AnalysisEntry _analysisEntry;

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public NPLSyntaxErrorTagger(NPLSyntaxErrorTaggerProvider provider, ITextBuffer buffer)
        {
            _buffer = buffer;
            _syntaxErrorTags = new List<ITagSpan<ErrorTag>>();
            _analysisEntry = buffer.GetAnalysisAtCaret(provider.ServiceProvider);
            _analysisEntry.NewParseTree += OnNewParseTree;
        }

        private void OnNewParseTree(object sender, ParseTreeChangedEventArgs e)
        {
            _syntaxErrorTags.Clear();
            _syntaxErrorMessages = e.Tree.ParserMessages;
            foreach (LogMessage syntaxErrorMessage in _syntaxErrorMessages)
            {
                _syntaxErrorTags.Add(new TagSpan<ErrorTag>(new SnapshotSpan(_buffer.CurrentSnapshot, Math.Min(_buffer.CurrentSnapshot.Length, Math.Max(syntaxErrorMessage.Location.Position, 1)) - 1, 1),
                    new ErrorTag("syntax error", syntaxErrorMessage.Message)));
            }

            if (TagsChanged != null)
                TagsChanged(this, new SnapshotSpanEventArgs(new SnapshotSpan(_buffer.CurrentSnapshot, new Span(0, _buffer.CurrentSnapshot.Length))));
        }

        IEnumerable<ITagSpan<ErrorTag>> ITagger<ErrorTag>.GetTags(NormalizedSnapshotSpanCollection spans)
        {
            int start = spans[0].Start.Position, end = spans[spans.Count - 1].End.Position;
            var res =  _syntaxErrorTags.Where(ts => ts.Span.End >= start && ts.Span.Start <= end).OfType<ITagSpan<ErrorTag>>();
            return res;
        }
    }
}
