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
using NPLTools.Intelligense;
using Microsoft.VisualStudio.Shell.Interop;

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
        IDictionary<TokenType, IClassificationType> _nplTypes;
        Irony.Parsing.Parser _parser;
        TokenList _tokens;
        AnalysisEntry _analysisEntry;
        /// <summary>
        /// Construct the classifier and define search tokens
        /// </summary>
        internal NPLClassifier(ITextBuffer textBuffer,
                               IClassificationTypeRegistryService typeService, NPLClassifierProvider provider)
        {
            _textBuffer = textBuffer;
            _parser = new Irony.Parsing.Parser(LuaGrammar.Instance);
            _tokens = new TokenList();

            IServiceProvider serviceProvider = provider.ServiceProvider as IServiceProvider;
            IVsSolution sln = serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
            var project = sln.GetLoadedProject().GetNPLProject();
            if (!project.GetAnalyzer().HasMonitoredTextBuffer(textBuffer))
                project.GetAnalyzer().MonitorTextBuffer(textBuffer);

            _analysisEntry = textBuffer.GetAnalysisAtCaret(provider.ServiceProvider);
            _analysisEntry.NewParseTree += OnNewParseTree;
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
            foreach (Token token in _tokens)
            {
                // if spanshot changed, but tokens not changed and exceed the length of snapshot, stop emunerate.
                if (token.Location.Position + token.Length >= spans[0].Snapshot.Length)
                    yield break;
                
                // Handle EOF token
                if (token.Category != TokenCategory.Outline && token.EditorInfo != null)
                    yield return
                        new TagSpan<ClassificationTag>(new SnapshotSpan(spans[0].Snapshot, new Span(token.Location.Position, token.Length)), new ClassificationTag(_nplTypes[token.EditorInfo.Type]));
            }
        }
    }
}
