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

            //ITextSnapshot currentSnapshot = subjectTriggerPoint.Value.Snapshot;
            //SnapshotSpan querySpan = new SnapshotSpan(subjectTriggerPoint.Value, 0);

            ITextStructureNavigator navigator = _provider.NavigatorService.GetTextStructureNavigator(_subjectBuffer);
            TextExtent extent = navigator.GetExtentOfWord(subjectTriggerPoint.Value);
            //string searchText = extent.Span.GetText();

            var analysis = _subjectBuffer.GetAnalysisAtCaret(_provider.ServiceProvider);
            string description = analysis.Analyzer.GetDescription(analysis, _subjectBuffer, subjectTriggerPoint.Value);

            if (description != String.Empty && description != null)
            {
                applicableToSpan = subjectTriggerPoint.Value.Snapshot.CreateTrackingSpan(extent.Span.Start, extent.Span.Length, SpanTrackingMode.EdgeInclusive);
                quickInfoContent.Add(description);
                return;
            }

            applicableToSpan = null;
        }

        private string GetDescription(string name, int position)
        {
            string description = String.Empty;
            List<KeyValuePair<LuaNode, Region>> declarationNodes = new List<KeyValuePair<LuaNode, Region>>();
            Parser parser = new Parser(LuaGrammar.Instance);
            LuaNode root = parser.Parse(_subjectBuffer.CurrentSnapshot.GetText()).Root.AstNode as LuaNode;
            GetDeclarationsByName(root, name, declarationNodes, position);
            declarationNodes.Sort(delegate (KeyValuePair<LuaNode, Region> a, KeyValuePair<LuaNode, Region> b)
            {
                if (b.Value.Contains(a.Value))
                    return -1;
                else
                    return 1;
            });

            if (declarationNodes.Count > 0 &&
                declarationNodes[0].Key is LuaFuncIdentifierNode)
            {
                //Parser parser = new Parser(LuaGrammar.Instance);
                Scanner scanner = new Parser(LuaGrammar.Instance).Scanner;

                int funcDefLine = declarationNodes[0].Key.Location.Line;
                for (int i = funcDefLine - 1; i >= 0; --i)
                {
                    string lineText = _subjectBuffer.CurrentSnapshot.GetLineFromLineNumber(i).GetText();
                    int state = 0;
                    scanner.VsSetSource(lineText, 0);
                    Token token = scanner.VsReadToken(ref state);
                    if (token == null || token.Terminal.Name != "block-comment")
                        break;
                    if (token.Terminal.Name == "block-comment")
                        description = (description == String.Empty) ? token.ValueString : token.ValueString + "\n" + description;
                }
            }
            return description;
        }

        private void GetDeclarationsByName(LuaNode node, string name, List<KeyValuePair<LuaNode, Region>> keyValue, int position)
        {
            if (node is LuaBlockNode)
            {
                foreach (var declaration in ((LuaBlockNode)node).Locals)
                {
                    if (declaration.AsString == name)
                    {
                        int scopeStartPosition = declaration.Span.EndPosition;
                        int scopeEndPosition = node.Span.EndPosition;

                        if (position >= scopeStartPosition &&
                            position <= scopeEndPosition)
                        {
                            keyValue.Add(new KeyValuePair<LuaNode, Region>(declaration, new Region(scopeStartPosition, scopeEndPosition)));
                        }
                        else if (position == scopeStartPosition)
                            keyValue.Add(new KeyValuePair<LuaNode, Region>(declaration, new Region(scopeStartPosition, scopeEndPosition)));
                    }
                }
            }
            foreach (LuaNode child in node.ChildNodes)
                GetDeclarationsByName(child, name, keyValue, position);
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

    // TODO: move the struct to somewhere more properly
    internal struct Region
    {
        public int start;
        public int end;
        public Region(int start, int end)
        {
            this.start = start;
            this.end = end;
        }

        public bool Contains(Region region)
        {
            if (this.start <= region.start && this.end >= region.end)
                return true;
            else
                return false;
        }
    }
}
