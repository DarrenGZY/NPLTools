using Irony.Interpreter.Ast;
using Irony.Parsing;
using Microsoft.VisualStudio.Text;
//using Microsoft.VisualStudio.Text.Editor;
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

        public static List<KeyValuePair<string, Span>> Declarations;

        public LuaModel(LuaNode root)
        {
            if (root != null)
            {
                _root = root;
                Declarations = new List<KeyValuePair<string, Span>>();
                GetDeclarations(_root, Declarations);
            } 
        }

        public void Update(LuaNode root)
        {
            if (root != null)
            {
                _root = root;
                Declarations.Clear();
                GetDeclarations(_root, Declarations);
            }  
        }

        //public bool IsInScope(int position, ITrackingSpan span)
        //{
        //    int line, index;
        //    _vsTextView.GetLineAndColumn(position, out line, out index);
        //    if ((line > span.iStartLine ||
        //        (line == span.iStartLine && index > span.iStartIndex)) &&
        //        (line < span.iEndLine ||
        //        (line == span.iEndLine && index < span.iEndIndex)))
        //        return true;
        //    return false;
        //}

        private void GetDeclarations(LuaNode node, List<KeyValuePair<string, Span>> declarations)
        {
            if (node is LuaBlockNode)
            {
                if (node.Parent is LuaChunkNode)
                {
                    foreach (var declaration in ((LuaBlockNode)node).Locals)
                    {
                        Span scope = new Span(declaration.Span.EndPosition, node.Span.EndPosition - declaration.Span.EndPosition);
                        //ITrackingSpan span = _textView.TextSnapshot.CreateTrackingSpan(scope, SpanTrackingMode.EdgeInclusive);

                        declarations.Add(
                            new KeyValuePair<string, Span>(
                                declaration.AsString, scope));
                    }
                }
                else
                {
                    foreach (var declaration in ((LuaBlockNode)node).Locals)
                    {
                        Span scope  = new Span(declaration.Span.EndPosition, node.Span.EndPosition - declaration.Span.EndPosition);

                        declarations.Add(
                            new KeyValuePair<string, Span>(
                                declaration.AsString, scope));
                    }
                } 
            }
            foreach (LuaNode child in node.ChildNodes)
                GetDeclarations(child, declarations);
        }

        public Span? GetDeclarationLocation(string name, Span span)
        {
            if (_root == null)
                return null;
            List<Span> spans = new List<Span>();
            GetDeclarationsByName(_root, name, spans, span);
            spans.Sort(delegate (Span a, Span b)
            {
                if (a.Start <= b.Start && a.Length <= b.Length)
                    return -1;
                else
                    return 1;
            });

            if (spans.Count > 0)
                return spans[0];

            return null;
        }

        //public string GetDescription(string name, Span span)
        //{
        //    string description = String.Empty;
        //    List<KeyValuePair<LuaNode, Span>> declarationNodes = new List<KeyValuePair<LuaNode, Span>>();
        //    GetDeclarationsByName(_root, name, declarationNodes, span);
        //    declarationNodes.Sort(delegate (KeyValuePair<LuaNode, Span> a, KeyValuePair<LuaNode, Span> b)
        //    {
        //        if ((a.Value.iStartLine > b.Value.iStartLine ||
        //            (a.Value.iStartLine == b.Value.iStartLine && a.Value.iStartIndex >= b.Value.iStartIndex)) &&
        //            (a.Value.iEndLine < b.Value.iEndLine ||
        //            (a.Value.iEndLine == b.Value.iEndLine && a.Value.iEndIndex <= b.Value.iEndIndex)))
        //            return -1;
        //        else
        //            return 1;
        //    });

        //    if (declarationNodes.Count > 0 &&
        //        declarationNodes[0].Key is LuaFuncIdentifierNode)
        //    {
        //        Parser parser = new Parser(LuaGrammar.Instance);
        //        Scanner scanner = parser.Scanner;

        //        int funcDefLine = declarationNodes[0].Key.Location.Line;
        //        for (int i = funcDefLine - 1; i >= 0; --i)
        //        {
        //            string lineText = _textView.TextSnapshot.GetLineFromLineNumber(i).GetText();
        //            int state = 0;
        //            scanner.VsSetSource(lineText, 0);
        //            Token token = scanner.VsReadToken(ref state);
        //            if (token.Terminal.Name == "block-comment")
        //                description += token.ValueString;
        //        }
        //    }
        //    return description;
        //}

        private void GetDeclarationsByName(AstNode node, string name, List<Span> spans, Span span)
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
                        Span declarationScope = new Span(startPosition, endPosition - startPosition);

                        if (span.Start >= declarationScope.Start &&
                            (span.Start + span.Length) <= declarationScope.Start + declarationScope.Length)
                            spans.Add(declarationScope);
                        else if (span.Start + span.Length == declarationScope.Start)
                            spans.Add(declarationScope);
                    }
                }
            }
            foreach (AstNode child in node.ChildNodes)
                GetDeclarationsByName(child, name, spans, span);
        }

        //private void GetDeclarationsByName(AstNode node, string name, List<KeyValuePair<LuaNode, TextSpan>> keyValue, TextSpan span)
        //{
        //    if (node is LuaBlockNode)
        //    {
        //        //node = node as LuaBlockNode;
        //        foreach (var declaration in ((LuaBlockNode)node).Locals)
        //        {
        //            if (declaration.AsString == name)
        //            {
        //                int startPosition = declaration.Span.EndPosition;
        //                int endPosition = node.Span.EndPosition;
        //                TextSpan declarationScope;
        //                _vsTextView.GetLineAndColumn(startPosition, out declarationScope.iStartLine, out declarationScope.iStartIndex);
        //                _vsTextView.GetLineAndColumn(endPosition, out declarationScope.iEndLine, out declarationScope.iEndIndex);

        //                if ((span.iStartLine > declarationScope.iStartLine ||
        //                    (span.iStartLine == declarationScope.iStartLine && span.iStartIndex >= declarationScope.iStartIndex)) &&
        //                    (span.iEndLine < declarationScope.iEndLine ||
        //                    (span.iEndLine == declarationScope.iEndLine && span.iEndIndex <= declarationScope.iEndIndex)))
        //                {
        //                    keyValue.Add(new KeyValuePair<LuaNode, TextSpan>(declaration, declarationScope));
        //                }
        //                else if (span.iEndLine == declarationScope.iStartLine &&
        //                    span.iEndIndex == declarationScope.iStartIndex)
        //                    keyValue.Add(new KeyValuePair<LuaNode, TextSpan>(declaration, declarationScope));
        //            }
        //        }
        //    }
        //    foreach (AstNode child in node.ChildNodes)
        //        GetDeclarationsByName(child, name, keyValue, span);
        //}

        private void RetrieveIndentationsFromSyntaxTree(out int[] indentations)
        {
            int lineNumber = view.TextSnapshot.LineCount;
            _root.
            indentations = new int[lineNumber];
            for (int i = 0; i < lineNumber; ++i)
            {
                indentations[i] = -1;
            }
            IterateAstTree(view, _root, indentations);
        }

        private void IterateAstTree(LuaNode node, int[] indentations)
        {
            if (node is LuaBlockNode)
            {
                for (int i = node.Span.Location.Line;
                    i <= view.TextSnapshot.GetLineNumberFromPosition(node.Span.EndPosition - 1); ++i)
                {
                    indentations[i] += 1;
                }
            }
            foreach (LuaNode childNode in node.GetChildNodes())
            {
                IterateAstTree(view, childNode, indentations);
            }
        }
    }
}
