//***************************************************************************
//
//    Copyright (c) Microsoft Corporation. All rights reserved.
//    This code is licensed under the Visual Studio SDK license terms.
//    THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//    ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//    IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//    PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//***************************************************************************

namespace NPL.Classifier
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Text;
    using Microsoft.VisualStudio.Text.Classification;
    using Microsoft.VisualStudio.Text.Editor;
    using Microsoft.VisualStudio.Text.Tagging;
    using Microsoft.VisualStudio.Utilities;
    using Irony.Parsing;
    using NPL.Parser;

    [Export(typeof(ITaggerProvider))]
    [ContentType("NPL")]
    [TagType(typeof(ClassificationTag))]
    internal sealed class NPLClassifierProvider : ITaggerProvider
    {

        [Export]
        [Name("NPL")]
        [BaseDefinition("code")]
        internal static ContentTypeDefinition NPLContentType = null;

        [Export]
        [FileExtension(".lua")]
        [ContentType("NPL")]
        internal static FileExtensionToContentTypeDefinition NPLFileType = null;

        [Import]
        internal IClassificationTypeRegistryService ClassificationTypeRegistry = null;

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            return new NPLClassifier(buffer, ClassificationTypeRegistry) as ITagger<T>;
        }
    }

    internal sealed class NPLClassifier : ITagger<ClassificationTag>
    {
        ITextBuffer _buffer;
        IDictionary<TokenColor, IClassificationType> _nplTypes;

        /// <summary>
        /// Construct the classifier and define search tokens
        /// </summary>
        internal NPLClassifier(ITextBuffer buffer,
                               IClassificationTypeRegistryService typeService)
        {
            _buffer = buffer;
            _nplTypes = new Dictionary<TokenColor, IClassificationType>();
            _nplTypes[TokenColor.Identifier] = typeService.GetClassificationType("Id");
            _nplTypes[TokenColor.Keyword] = typeService.GetClassificationType("Keyword");
            _nplTypes[TokenColor.String] = typeService.GetClassificationType("String");
            _nplTypes[TokenColor.Comment] = typeService.GetClassificationType("Comment");
            _nplTypes[TokenColor.Number] = typeService.GetClassificationType("Number");
            _nplTypes[TokenColor.Text] = typeService.GetClassificationType("Text");
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged
        {
            add { }
            remove { }
        }

        /// <summary>
        /// Search the given span for any instances of classified tags
        /// </summary>
        public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            Irony.Parsing.Parser parser = new Irony.Parsing.Parser(LuaGrammar.Instance);
            foreach (SnapshotSpan curSpan in spans)
            {
                ITextSnapshotLine containingLine = curSpan.Start.GetContainingLine();
                parser.Scanner.VsSetSource(containingLine.GetText(), 0);
                //ParseTree tree = parser.Parse(containingLine.GetText());
                //ParseTreeNode root = tree.Root;
                //var node = root.AstNode;
                int state = 0;
                Token token = parser.Scanner.VsReadToken(ref state);
                while(token != null)
                {
                    yield return
                        new TagSpan<ClassificationTag>(new SnapshotSpan(curSpan.Snapshot, new Span(curSpan.Start.Position + token.Location.Position, token.Length)), new ClassificationTag(_nplTypes[token.EditorInfo.Color]));
                    token = parser.Scanner.VsReadToken(ref state);
                }
            }
        }
    }
}
