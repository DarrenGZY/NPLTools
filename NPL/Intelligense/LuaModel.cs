using Irony.Interpreter.Ast;
using Microsoft.VisualStudio.TextManager.Interop;
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
        private AstNode _root;
        private IVsTextView _vsTextView;
        public LuaModel(IVsTextView vsTextView, AstNode root)
        {
            _vsTextView = vsTextView;
            _root = root;
        }

        public void Update(AstNode root)
        {
            _root = root;
        }

        public TextSpan? GetDeclarationLocation(string name, TextSpan span)
        {
            List<TextSpan> spans = new List<TextSpan>();
            GetDeclarationsByName(_root, name, spans, span);
            spans.Sort(delegate (TextSpan a, TextSpan b)
            {
                if ((a.iStartLine > b.iStartLine || 
                    (a.iStartLine == b.iStartLine && a.iStartIndex > b.iStartIndex)) && 
                    (a.iEndLine < b.iEndLine ||
                    (a.iEndLine == b.iEndLine && a.iEndIndex < b.iEndIndex)))
                    return -1;
                else
                    return 1;
            });

            if (spans.Count > 0)
                return spans[0];

            return null;
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
                            (span.iStartLine == declarationScope.iStartLine && span.iStartIndex > declarationScope.iStartIndex)) &&
                            (span.iEndLine < declarationScope.iEndLine ||
                            (span.iEndLine == declarationScope.iEndLine && span.iEndIndex < declarationScope.iEndIndex)))
                            spans.Add(declarationScope);
                    }

                }
            }
            //else if (node is LuaDoBlockNode)
            //{
            //    //node = node as LuaBlockNode;
            //    foreach (var declaration in ((LuaDoBlockNode)node).Locals)
            //    {
            //        foreach (var variable in declaration.Variables)
            //        {
            //            if (variable.Key == name)
            //            {
            //                int startPosition = variable.Value.Span.EndPosition;
            //                int endPosition = node.Span.EndPosition;
            //                TextSpan span;
            //                _vsTextView.GetLineAndColumn(startPosition, out span.iStartLine, out span.iStartIndex);
            //                _vsTextView.GetLineAndColumn(endPosition, out span.iEndLine, out span.iEndIndex);
            //                spans.Add(span);
            //            }
            //        }
            //    }
            //}
            foreach (AstNode child in node.ChildNodes)
                GetDeclarationsByName(child, name, spans, span);
        }
    }
}
