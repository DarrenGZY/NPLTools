using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using Irony.Parsing;
using Irony;
using System.Linq;
using NPL.Parser;
using NPLTools.Language.Editor;

namespace NPL.Classifier
{
    [Export(typeof(ITaggerProvider))]
    [ContentType("NPL")]
    [TagType(typeof(ClassificationTag))]
    [TagType(typeof(ErrorTag))]
    internal sealed class NPLClassifierProvider : ITaggerProvider
    {
        [Export]
        [Name("NPL")]
        [BaseDefinition("code")]
        internal static ContentTypeDefinition NPLContentType = null;

        [Export]
        [FileExtension(".npl")]
        [ContentType("NPL")]
        internal static FileExtensionToContentTypeDefinition NPLFileType = null;

        [Import]
        internal IClassificationTypeRegistryService ClassificationTypeRegistry = null;

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            return new NPLClassifier(buffer, ClassificationTypeRegistry) as ITagger<T>;
        }
    }

    internal sealed class NPLClassifier : ITagger<ClassificationTag>, ITagger<ErrorTag>
    {
        ITextBuffer _buffer;
        IDictionary<TokenType, IClassificationType> _nplTypes;
        List<ITagSpan<ErrorTag>> _syntaxErrorTags;
        LogMessageList _syntaxErrorMessages;
        Irony.Parsing.Parser _parser = new Irony.Parsing.Parser(LuaGrammar.Instance);
        string _currentText;
        /// <summary>
        /// Construct the classifier and define search tokens
        /// </summary>
        internal NPLClassifier(ITextBuffer buffer,
                               IClassificationTypeRegistryService typeService)
        {
            _buffer = buffer;
            //_buffer.Changed += TextChanged;
            _currentText = _buffer.CurrentSnapshot.GetText();
            _syntaxErrorTags = new List<ITagSpan<ErrorTag>>();
            _nplTypes = new Dictionary<TokenType, IClassificationType>();
            _nplTypes[TokenType.Identifier] = typeService.GetClassificationType("Id");
            _nplTypes[TokenType.Keyword] = typeService.GetClassificationType("Keyword");
            _nplTypes[TokenType.String] = typeService.GetClassificationType("String");
            _nplTypes[TokenType.LineComment] = typeService.GetClassificationType("Comment");
            _nplTypes[TokenType.Comment] = typeService.GetClassificationType("Comment");
            _nplTypes[TokenType.Delimiter] = typeService.GetClassificationType("Text");
            _nplTypes[TokenType.Literal] = typeService.GetClassificationType("Text");
            _nplTypes[TokenType.Operator] = typeService.GetClassificationType("Text");
            _nplTypes[TokenType.WhiteSpace] = typeService.GetClassificationType("Text");
            _nplTypes[TokenType.Unknown] = typeService.GetClassificationType("Text");
            NPLTextViewCreationListener.TextContentChanged += TextContentChanged;
        }

        private void TextContentChanged(object sender, NPLTextContentChangedEventArgs e)
        {
            _syntaxErrorTags.Clear();
            _syntaxErrorMessages = _parser.Parse(e.Snapshot.GetText()).ParserMessages;
            foreach (LogMessage syntaxErrorMessage in _syntaxErrorMessages)
            {
                _syntaxErrorTags.Add(new TagSpan<ErrorTag>(new SnapshotSpan(e.Snapshot, Math.Min(e.Snapshot.Length, syntaxErrorMessage.Location.Position) - 1, 1),
                    new ErrorTag("syntax error", syntaxErrorMessage.Message)));
            }

            if (TagsChanged != null)
                TagsChanged(this, new SnapshotSpanEventArgs(new SnapshotSpan(e.Snapshot, new Span(0, e.Snapshot.Length))));
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        /// <summary>
        /// Search the given span for any instances of classified tags
        /// </summary>
        public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            foreach (SnapshotSpan curSpan in spans)
            {
                ITextSnapshotLine containingLine = curSpan.Start.GetContainingLine();
                TokenList tokens = _parser.Parse(containingLine.GetText()).Tokens;
                foreach(Token token in tokens)
                {
                    yield return
                        new TagSpan<ClassificationTag>(new SnapshotSpan(curSpan.Snapshot, new Span(curSpan.Start.Position + token.Location.Position, token.Length)), new ClassificationTag(_nplTypes[token.EditorInfo.Type]));
                }
            }
        }

        IEnumerable<ITagSpan<ErrorTag>> ITagger<ErrorTag>.GetTags(NormalizedSnapshotSpanCollection spans)
        {
            int start = spans[0].Start.Position, end = spans[spans.Count - 1].End.Position;
            return _syntaxErrorTags.Where(ts => ts.Span.End >= start && ts.Span.Start <= end).OfType<ITagSpan<ErrorTag>>();
        }
    }
}
