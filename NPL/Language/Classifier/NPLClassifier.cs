using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using Irony.Parsing;
using NPLTools.IronyParser;
using Microsoft.VisualStudio.Shell;
using NPLTools.Intellisense;
using Microsoft.VisualStudio.Shell.Interop;
using NPLTools.Project;

namespace NPLTools.Language.Classifier
{
    [Export(typeof(ITaggerProvider))]
    [ContentType("NPL")]
    [TagType(typeof(ClassificationTag))]
    //[TagType(typeof(ErrorTag))]
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

        [Export]
        [FileExtension(".lua")]
        [ContentType("NPL")]
        internal static FileExtensionToContentTypeDefinition LuaFileType = null;

        [Import]
        internal IClassificationTypeRegistryService ClassificationTypeRegistry = null;

        [Import]
        internal SVsServiceProvider ServiceProvider { get; set; }

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            return new NPLClassifier(buffer, ClassificationTypeRegistry, this) as ITagger<T>;
        }
    }

    internal sealed class NPLClassifier : ITagger<ClassificationTag>
    {
        ITextBuffer _textBuffer;
        IDictionary<NPLTokenType, IClassificationType> _nplTypes;
        TokenList _tokens;
        AnalysisEntry _analysisEntry;
        /// <summary>
        /// Construct the classifier and define search tokens
        /// </summary>
        internal NPLClassifier(ITextBuffer textBuffer,
                               IClassificationTypeRegistryService typeService, NPLClassifierProvider provider)
        {
            _textBuffer = textBuffer;
            _tokens = new TokenList();

            IServiceProvider serviceProvider = provider.ServiceProvider as IServiceProvider;
            _analysisEntry = AnalysisEntryInitializer.Initialize(serviceProvider, textBuffer);
            _analysisEntry.NewParseTree += OnNewParseTree;
            InitializeTypeMapping(typeService);
        }

        private void OnNewParseTree(object sender, ParseTreeChangedEventArgs e)
        {
            if (e.Tree != null && e.Tree.Root != null)
            {
                _tokens = e.Tree.Tokens;
            }
            if (TagsChanged != null)
                TagsChanged(this, new SnapshotSpanEventArgs(new SnapshotSpan(_textBuffer.CurrentSnapshot, 0, _textBuffer.CurrentSnapshot.Length)));
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        /// <summary>
        /// Search the given span for any instances of classified tags
        /// </summary>
        public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans == null || spans.Count == 0)
                yield break;
            for (int i = 0; i < _tokens.Count; ++i)
            {
                Token token = _tokens[i];
                Token nextToken = (i + 1) == _tokens.Count ? null : _tokens[i + 1];
                // if spanshot changed, but tokens not changed and exceed the length of snapshot, stop emunerate.
                if (token.Location.Position + token.Length >= spans[0].Snapshot.Length)
                    yield break;
                
                // Handle EOF token
                if (token.Category != TokenCategory.Outline && token.EditorInfo != null)
                    yield return
                        new TagSpan<ClassificationTag>(new SnapshotSpan(spans[0].Snapshot, new Span(token.Location.Position, token.Length)), new ClassificationTag(_nplTypes[GetNPLTokenType(token, nextToken)]));
            }
        }

        private NPLTokenType GetNPLTokenType(Token token, Token nextToken)
        {
            NPLTokenType res;
            switch (token.EditorInfo.Type)
            {
                case TokenType.Text:
                case TokenType.Literal:
                case TokenType.Operator:
                case TokenType.WhiteSpace:
                case TokenType.Unknown:
                case TokenType.Delimiter:
                    res = NPLTokenType.Text;
                    break;
                case TokenType.String:
                    res = NPLTokenType.String;
                    break;
                case TokenType.LineComment:
                case TokenType.Comment:
                    res = NPLTokenType.Comment;
                    break;
                case TokenType.Identifier:
                    if (nextToken.Text == "(" && 
                        nextToken.EditorInfo.Type == TokenType.Delimiter)
                    {
                        res = NPLTokenType.FunctionName;
                    }
                    else if (token.Text == "self")
                    {
                        res = NPLTokenType.Self;
                    }
                    else
                    {
                        res = NPLTokenType.Identifier;
                    }
                    break;
                case TokenType.Keyword:
                    res = NPLTokenType.Keyword;
                    break;
                default:
                    res = NPLTokenType.Text;
                    break;
            }

            return res;
        }

        private void InitializeTypeMapping(IClassificationTypeRegistryService typeService)
        {

            _nplTypes = new Dictionary<NPLTokenType, IClassificationType>();
            _nplTypes[NPLTokenType.Text] = typeService.GetClassificationType("NPLText");
            _nplTypes[NPLTokenType.Keyword] = typeService.GetClassificationType("NPLKeyword");
            _nplTypes[NPLTokenType.String] = typeService.GetClassificationType("NPLString");
            _nplTypes[NPLTokenType.Comment] = typeService.GetClassificationType("NPLComment");
            _nplTypes[NPLTokenType.Identifier] = typeService.GetClassificationType("NPLIdentifier");
            _nplTypes[NPLTokenType.Number] = typeService.GetClassificationType("NPLNumber");
            _nplTypes[NPLTokenType.Self] = typeService.GetClassificationType("NPLSelf");
            _nplTypes[NPLTokenType.FunctionName] = typeService.GetClassificationType("NPLFunctionName");
        }
    }

    public enum NPLTokenType
    {
        Text = 1,
        Keyword = 2,
        Identifier = 3,
        String = 4,
        Comment = 5,
        Number = 6,
        Self = 7,
        FunctionName = 8,
    }
}
