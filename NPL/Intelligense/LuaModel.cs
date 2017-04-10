using Irony.Interpreter.Ast;
using Irony.Parsing;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using NPLTools.IronyParser;
using NPLTools.IronyParser.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLTools.Intelligense
{
    public class LuaModel
    {
        private LuaNode _root;
        private ITextView _textView;
        private static IVsTextView _vsTextView;

        public static List<KeyValuePair<string, TextSpan>> Declarations;

        public LuaModel(ITextView textView, IVsTextView vsTextView,LuaNode root)
        {
            _textView = textView;
            _vsTextView = vsTextView;
            _root = root;
            Declarations = new List<KeyValuePair<string, TextSpan>>();
            //GlobalDeclarations = new List<KeyValuePair<string, TextSpan>>();
            GetDeclarations(_root, Declarations);
        }

        public void Update(LuaNode root)
        {
            _root = root;
            Declarations.Clear();
            GetDeclarations(_root, Declarations);
        }

        public static bool IsInScope(int position, TextSpan span)
        {
            int line, index;
            _vsTextView.GetLineAndColumn(position, out line, out index);
            if ((line > span.iStartLine ||
                (line == span.iStartLine && index > span.iStartIndex)) &&
                (line < span.iEndLine ||
                (line == span.iEndLine && index < span.iEndIndex)))
                return true;
            return false;
        }

        private void GetDeclarations(LuaNode node, List<KeyValuePair<string, TextSpan>> declarations)
        {
            if (node is LuaBlockNode)
            {
                if (node.Parent is LuaChunkNode)
                {
                    foreach (var declaration in ((LuaBlockNode)node).Locals)
                    {
                        int startLine, startIndex;
                        _vsTextView.GetLineAndColumn(declaration.Span.EndPosition, out startLine, out startIndex);
                        TextSpan scope;
                        scope.iStartLine = startLine;
                        scope.iStartIndex = startIndex;
                        scope.iEndLine = int.MaxValue;
                        scope.iEndIndex = int.MaxValue;

                        declarations.Add(
                            new KeyValuePair<string, TextSpan>(
                                declaration.AsString, scope));
                    }
                }
                else
                {
                    foreach (var declaration in ((LuaBlockNode)node).Locals)
                    {
                        int startLine, startIndex;
                        int endLine, endIndex;
                        _vsTextView.GetLineAndColumn(declaration.Span.EndPosition, out startLine, out startIndex);
                        _vsTextView.GetLineAndColumn(((LuaBlockNode)node).Scope.endPosition, out endLine, out endIndex);
                        TextSpan scope;
                        scope.iStartLine = startLine;
                        scope.iStartIndex = startIndex;
                        scope.iEndLine = endLine;
                        scope.iEndIndex = endIndex;

                        declarations.Add(
                            new KeyValuePair<string, TextSpan>(
                                declaration.AsString, scope));
                    }
                } 
            }
            foreach (LuaNode child in node.ChildNodes)
                GetDeclarations(child, declarations);
        }

        public TextSpan? GetDeclarationLocation(string name, TextSpan span)
        {
            List<TextSpan> spans = new List<TextSpan>();
            GetDeclarationsByName(_root, name, spans, span);
            spans.Sort(delegate (TextSpan a, TextSpan b)
            {
                if ((a.iStartLine > b.iStartLine || 
                    (a.iStartLine == b.iStartLine && a.iStartIndex >= b.iStartIndex)) && 
                    (a.iEndLine < b.iEndLine ||
                    (a.iEndLine == b.iEndLine && a.iEndIndex <= b.iEndIndex)))
                    return -1;
                else
                    return 1;
            });

            if (spans.Count > 0)
                return spans[0];

            return null;
        }

        public string GetDescription(string name, TextSpan span)
        {
            string description = String.Empty;
            List<KeyValuePair<LuaNode, TextSpan>> declarationNodes = new List<KeyValuePair<LuaNode, TextSpan>>();
            GetDeclarationsByName(_root, name, declarationNodes, span);
            declarationNodes.Sort(delegate (KeyValuePair<LuaNode, TextSpan> a, KeyValuePair<LuaNode, TextSpan> b)
            {
                if ((a.Value.iStartLine > b.Value.iStartLine ||
                    (a.Value.iStartLine == b.Value.iStartLine && a.Value.iStartIndex >= b.Value.iStartIndex)) &&
                    (a.Value.iEndLine < b.Value.iEndLine ||
                    (a.Value.iEndLine == b.Value.iEndLine && a.Value.iEndIndex <= b.Value.iEndIndex)))
                    return -1;
                else
                    return 1;
            });

            if (declarationNodes.Count > 0 &&
                declarationNodes[0].Key is LuaFuncIdentifierNode)
            {
                Parser parser = new Parser(LuaGrammar.Instance);
                Scanner scanner = parser.Scanner;

                int funcDefLine = declarationNodes[0].Key.Location.Line;
                for (int i = funcDefLine - 1; i >= 0; --i)
                {
                    string lineText = _textView.TextSnapshot.GetLineFromLineNumber(i).GetText();
                    int state = 0;
                    scanner.VsSetSource(lineText, 0);
                    Token token = scanner.VsReadToken(ref state);
                    if (token.Terminal.Name == "block-comment")
                        description += token.ValueString;
                }
            }
            return description;
        }

        private void GetDeclarationsByName(AstNode node, string name, List<TextSpan> spans, TextSpan span)
        {
            if (node is LuaBlockNode)
            {
                //node = node as LuaBlockNode;
                foreach (var declaration in ((LuaBlockNode)node).Locals)
                {
                    if (declaration.AsString == name)
                    {
                        int startPosition = declaration.Span.EndPosition;
                        int endPosition = node.Span.EndPosition;
                        TextSpan declarationScope;
                        _vsTextView.GetLineAndColumn(startPosition, out declarationScope.iStartLine, out declarationScope.iStartIndex);
                        _vsTextView.GetLineAndColumn(endPosition, out declarationScope.iEndLine, out declarationScope.iEndIndex);

                        if ((span.iStartLine > declarationScope.iStartLine ||
                            (span.iStartLine == declarationScope.iStartLine && span.iStartIndex >= declarationScope.iStartIndex)) &&
                            (span.iEndLine < declarationScope.iEndLine ||
                            (span.iEndLine == declarationScope.iEndLine && span.iEndIndex <= declarationScope.iEndIndex)))
                            spans.Add(declarationScope);
                        else if (span.iEndLine == declarationScope.iStartLine &&
                            span.iEndIndex == declarationScope.iStartIndex)
                            spans.Add(declarationScope);
                    }
                }
            }
            foreach (AstNode child in node.ChildNodes)
                GetDeclarationsByName(child, name, spans, span);
        }

        private void GetDeclarationsByName(AstNode node, string name, List<KeyValuePair<LuaNode, TextSpan>> keyValue, TextSpan span)
        {
            if (node is LuaBlockNode)
            {
                //node = node as LuaBlockNode;
                foreach (var declaration in ((LuaBlockNode)node).Locals)
                {
                    if (declaration.AsString == name)
                    {
                        int startPosition = declaration.Span.EndPosition;
                        int endPosition = node.Span.EndPosition;
                        TextSpan declarationScope;
                        _vsTextView.GetLineAndColumn(startPosition, out declarationScope.iStartLine, out declarationScope.iStartIndex);
                        _vsTextView.GetLineAndColumn(endPosition, out declarationScope.iEndLine, out declarationScope.iEndIndex);

                        if ((span.iStartLine > declarationScope.iStartLine ||
                            (span.iStartLine == declarationScope.iStartLine && span.iStartIndex >= declarationScope.iStartIndex)) &&
                            (span.iEndLine < declarationScope.iEndLine ||
                            (span.iEndLine == declarationScope.iEndLine && span.iEndIndex <= declarationScope.iEndIndex)))
                        {
                            keyValue.Add(new KeyValuePair<LuaNode, TextSpan>(declaration, declarationScope));
                        }
                        else if (span.iEndLine == declarationScope.iStartLine &&
                            span.iEndIndex == declarationScope.iStartIndex)
                            keyValue.Add(new KeyValuePair<LuaNode, TextSpan>(declaration, declarationScope));
                    }
                }
            }
            foreach (AstNode child in node.ChildNodes)
                GetDeclarationsByName(child, name, keyValue, span);
        }
    }
}
