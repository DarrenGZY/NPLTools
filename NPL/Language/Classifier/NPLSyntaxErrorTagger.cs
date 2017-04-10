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


namespace NPLTools.Language.Classifier
{
    [Export(typeof(ITaggerProvider))]
    [ContentType("NPL")]
    [TagType(typeof(ErrorTag))]
    internal sealed class NPLSyntaxErrorTaggerProvider : ITaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            return new NPLSyntaxErrorTagger(buffer) as ITagger<T>;
        }
    }

    internal sealed class NPLSyntaxErrorTagger : ITagger<ErrorTag>
    {
        private ITextBuffer _buffer;
        private Parser _parser;
        private List<ITagSpan<ErrorTag>> _syntaxErrorTags;
        private LogMessageList _syntaxErrorMessages;

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public NPLSyntaxErrorTagger(ITextBuffer buffer)
        {
            _buffer = buffer;
            _syntaxErrorTags = new List<ITagSpan<ErrorTag>>();
            _parser = new Parser(LuaGrammar.Instance);
            NPLTextViewCreationListener.TextContentChanged += TextContentChanged;
        }

        IEnumerable<ITagSpan<ErrorTag>> ITagger<ErrorTag>.GetTags(NormalizedSnapshotSpanCollection spans)
        {
            int start = spans[0].Start.Position, end = spans[spans.Count - 1].End.Position;
            var res =  _syntaxErrorTags.Where(ts => ts.Span.End >= start && ts.Span.Start <= end).OfType<ITagSpan<ErrorTag>>();
            return res;
        }

        private void TextContentChanged(object sender, NPLTextContentChangedEventArgs e)
        {
            if (_buffer.CurrentSnapshot != e.Snapshot)
                return;
            _syntaxErrorTags.Clear();
            _syntaxErrorMessages = _parser.Parse(e.Snapshot.GetText()).ParserMessages;
            foreach (LogMessage syntaxErrorMessage in _syntaxErrorMessages)
            {
                _syntaxErrorTags.Add(new TagSpan<ErrorTag>(new SnapshotSpan(e.Snapshot, Math.Min(e.Snapshot.Length, Math.Max(syntaxErrorMessage.Location.Position, 1)) - 1, 1),
                    new ErrorTag("syntax error", syntaxErrorMessage.Message)));
            }

            if (TagsChanged != null)
                TagsChanged(this, new SnapshotSpanEventArgs(new SnapshotSpan(e.Snapshot, new Span(0, e.Snapshot.Length))));
        }
    }
}
