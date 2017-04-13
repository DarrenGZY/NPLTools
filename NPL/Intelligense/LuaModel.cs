using Irony.Interpreter.Ast;
using Irony.Parsing;
//using Microsoft.VisualStudio.Text;
//using Microsoft.VisualStudio.Text.Editor;
using NPLTools.IronyParser;
using NPLTools.IronyParser.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SourceSpan = NPLTools.Intelligense.SourceSpan;

namespace NPLTools.Intelligense
{
    public class LuaModel
    {
        private LuaNode _root;
        public static List<KeyValuePair<string, SourceSpan>> Declarations;

        public LuaModel(LuaNode root)
        {
            if (root != null)
            {
                _root = root;
                Declarations = new List<KeyValuePair<string, SourceSpan>>();
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

        private void GetDeclarations(LuaNode node, List<KeyValuePair<string, SourceSpan>> declarations)
        {
            if (node is LuaBlockNode)
            {
                if (node.Parent is LuaChunkNode)
                {
                    foreach (var declaration in ((LuaBlockNode)node).Locals)
                    {
                        SourceSpan scope = new SourceSpan(declaration.Span.EndPosition,
                            declaration.EndLine,
                            node.Span.EndPosition,
                            node.EndLine);

                        declarations.Add(
                            new KeyValuePair<string, SourceSpan>(
                                declaration.AsString, scope));
                    }
                }
                else
                {
                    foreach (var declaration in ((LuaBlockNode)node).Locals)
                    {
                        SourceSpan scope  = new SourceSpan(declaration.Span.EndPosition, 
                            declaration.EndLine,
                            node.Span.EndPosition,
                            node.EndLine);

                        declarations.Add(
                            new KeyValuePair<string, SourceSpan>(
                                declaration.AsString, scope));
                    }
                } 
            }
            foreach (LuaNode child in node.ChildNodes)
                GetDeclarations(child, declarations);
        }

        public SourceSpan? GetDeclarationLocation(string name, SourceSpan span)
        {
            if (_root == null)
                return null;
            List<SourceSpan> spans = new List<SourceSpan>();
            GetDeclarationsByName(_root, name, spans, span);
            spans.Sort(delegate (SourceSpan a, SourceSpan b)
            {
                if (a.StartPosition >= b.StartPosition && a.EndPosition <= b.EndPosition)
                    return -1;
                else
                    return 1;
            });

            if (spans.Count > 0)
                return spans[0];

            return null;
        }

        private void GetDeclarationsByName(LuaNode node, string name, List<SourceSpan> spans, SourceSpan span)
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
                        SourceSpan declarationScope = new SourceSpan(startPosition, 
                            declaration.EndLine,
                            endPosition,
                            node.EndLine);

                        if (span.StartPosition >= declarationScope.StartPosition &&
                            span.EndPosition <= declarationScope.EndPosition)
                            spans.Add(declarationScope);
                        else if (span.EndPosition == declarationScope.StartPosition)
                            spans.Add(declarationScope);
                    }
                }
            }
            foreach (LuaNode child in node.ChildNodes)
                GetDeclarationsByName(child, name, spans, span);
        }

        public void RetrieveIndentationsFromSyntaxTree(out int[] indentations)
        {
            int lineNumber = _root.EndLine + 1;
            indentations = new int[lineNumber];
            for (int i = 0; i < lineNumber; ++i)
            {
                indentations[i] = -1;
            }
            WalkSyntaxTreeForIndentations(_root, indentations);
        }

        private void WalkSyntaxTreeForIndentations(LuaNode node, int[] indentations)
        {
            if (node is LuaBlockNode)
            {
                for (int i = node.Span.Location.Line;
                    i <= node.EndLine; ++i)
                {
                    indentations[i] += 1;
                }
            }
            foreach (LuaNode childNode in node.GetChildNodes())
            {
                WalkSyntaxTreeForIndentations(childNode, indentations);
            }
        }
    }
}
