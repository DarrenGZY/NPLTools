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
        private LuaChunkNode _root;
        private ParseTree _parseTree;
        public List<KeyValuePair<string, ScopeSpan>> Declarations = new List<KeyValuePair<string, ScopeSpan>>();

        public LuaModel(ParseTree parseTree)
        {
            _parseTree = parseTree;

            if (_parseTree.Root!= null)
            {
                _root = _parseTree.Root.AstNode as LuaChunkNode;
                GetDeclarations(_root, Declarations);
            }
        }

        public void Update(ParseTree parseTree)
        {
            _parseTree = parseTree;

            if (_parseTree.Root != null && _parseTree.Root.AstNode != null)
            {
                _root = _parseTree.Root.AstNode as LuaChunkNode;
                Declarations.Clear();
                GetDeclarations(_root, Declarations);  
            }
        }

        public bool IsInScope(int position, ScopeSpan span)
        {
            if (position >= span.StartPosition &&
                position <= span.EndPosition)
                return true;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ScopeSpan? GetGlobalDeclarationLocation(string name)
        {
            if (_root == null)
                return null;
            List<ScopeSpan> spans = new List<ScopeSpan>();
            GetGlobalDeclarationsByName(_root, name, spans);
            spans.Sort(delegate (ScopeSpan a, ScopeSpan b)
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

        private void GetDeclarations(LuaNode node, List<KeyValuePair<string, ScopeSpan>> declarations)
        {
            if (node == null)
                return;

            if (node is LuaBlockNode)
            {
                if (node.Parent is LuaChunkNode)
                {
                    foreach (var declaration in ((LuaBlockNode)node).Locals)
                    {
                        declarations.Add(
                            new KeyValuePair<string, ScopeSpan>(
                                declaration.Name, declaration.Scope));
                    }
                }
                else
                {
                    foreach (var declaration in ((LuaBlockNode)node).Locals)
                    {
                        declarations.Add(
                            new KeyValuePair<string, ScopeSpan>(
                                declaration.Name, declaration.Scope));
                    }
                } 
            }
            foreach (LuaNode child in node.ChildNodes)
                GetDeclarations(child, declarations);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="span"></param>
        /// <returns></returns>
        public ScopeSpan? GetDeclarationLocation(string name, ScopeSpan span)
        {
            if (_root == null)
                return null;
            List<ScopeSpan> spans = new List<ScopeSpan>();
            GetDeclarationsByName(_root, name, spans, span);
            spans.Sort(delegate (ScopeSpan a, ScopeSpan b)
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

        private void GetDeclarationsByName(LuaNode node, string name, List<ScopeSpan> spans, ScopeSpan span)
        {
            if (node is LuaBlockNode)
            {
                //node = node as LuaBlockNode;
                foreach (var declaration in ((LuaBlockNode)node).Locals)
                {
                    if (declaration.NamesEqual(new List<string>(name.Split('.'))))
                    {
                        if (span.StartPosition >= declaration.Scope.StartPosition &&
                            span.EndPosition <= declaration.Scope.EndPosition)
                            spans.Add(declaration.Scope);
                        else if (span.EndPosition == declaration.Scope.StartPosition)
                            spans.Add(declaration.Scope);
                    }
                }

                foreach (var declaration in ((LuaBlockNode)node).Globals)
                {
                    if (declaration.NamesEqual(new List<string>(name.Split('.'))))
                    {
                        if (span.StartPosition >= declaration.Scope.StartPosition &&
                            span.EndPosition <= declaration.Scope.EndPosition)
                            spans.Add(declaration.Scope);
                        else if (span.EndPosition == declaration.Scope.StartPosition)
                            spans.Add(declaration.Scope);
                    }
                }
            }
            foreach (LuaNode child in node.ChildNodes)
                GetDeclarationsByName(child, name, spans, span);
        }

        private void GetGlobalDeclarationsByName(LuaNode node, string name, List<ScopeSpan> spans)
        {
            if (node is LuaBlockNode)
            {
                foreach (var declaration in ((LuaBlockNode)node).Globals)
                {
                    if (declaration.NamesEqual(new List<string>(name.Split('.'))))
                    {
                            spans.Add(declaration.Scope);
                    }
                }
            }
            foreach (LuaNode child in node.ChildNodes)
                GetGlobalDeclarationsByName(child, name, spans);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="indentations"></param>
        public void RetrieveIndentationsFromSyntaxTree(out int[] indentations, out bool[] fixedLines)
        {
            int lineNumber = _parseTree.GetEndLine() + 1;
            indentations = new int[lineNumber];
            fixedLines = new bool[lineNumber];
            for (int i = 0; i < lineNumber; ++i)
            {
                indentations[i] = -1;
                fixedLines[i] = false;
            }
            WalkSyntaxTreeForIndentations(_root, indentations);

            for (int i = 0; i < _parseTree.Tokens.Count; ++i)
            {
                var curToken = _parseTree.Tokens[i];
                if (curToken.Terminal.Name == "block-comment" ||
                    curToken.Terminal.Name == "long-string")
                {
                    // check if comment token is the first token of the line
                    if (i > 0)
                    {
                        var lastToken = _parseTree.Tokens[i - 1];
                        // if comment token is not the first token, continue
                        if (lastToken.Location.Line != curToken.Location.Line)
                            fixedLines[curToken.Location.Line] = true;
                    }

                    // there would always be a next token, because last token would always be EOF
                    var nextToken = _parseTree.Tokens[i + 1];
                    for (int j = curToken.Location.Line + 1; j < nextToken.Location.Line; ++j)
                    {
                        fixedLines[j] = true;
                    }
                }
            }
        }

        private void WalkSyntaxTreeForIndentations(LuaNode node, int[] indentations)
        {
            if (node is LuaBlockNode)
            {
                // if block span occupys two lines or more, add indentation for it
                if (node.Span.Location.Line == node.EndLine)
                {
                    if (node.Span.Location.Line != node.Parent.Span.Location.Line)
                        indentations[node.Span.Location.Line] += 1;
                }               

                if (node.Span.Location.Line != node.EndLine)
                {
                    for (int i = node.Span.Location.Line;
                    i <= node.EndLine; ++i)
                    {
                        indentations[i] += 1;
                    }
                } 
            }
            foreach (LuaNode childNode in node.GetChildNodes())
            {
                WalkSyntaxTreeForIndentations(childNode, indentations);
            }
        }

        public List<Region> GetOutliningRegions()
        {
            List<Region> regions = new List<Region>();

            if (_root != null)
                WalkSyntaxTreeForOutliningRegions(_parseTree.Root, regions);

            return regions;
        }

        private void WalkSyntaxTreeForOutliningRegions(ParseTreeNode node, List<Region> regions)
        {
            if (node == null) return;

            int startPosition, length;
            if (node.Term.Name == NPLConstants.FunctionDeclaration)
            {
                startPosition = node.ChildNodes[2].Span.EndPosition + 1;
                length = node.ChildNodes[4].Span.Location.Position - startPosition;
                regions.Add(new Region(startPosition, length));
            }
            else if (node.Term.Name == NPLConstants.LocalFunctionDeclaration)
            {
                startPosition = node.ChildNodes[3].Span.EndPosition + 1;
                length = node.ChildNodes[5].Span.Location.Position - startPosition;
                regions.Add(new Region(startPosition, length));
            }
            else if (node.Term.Name == NPLConstants.DoBlock)
            {
                startPosition = node.ChildNodes[0].Span.EndPosition;
                length = node.ChildNodes[2].Span.Location.Position - node.ChildNodes[0].Span.EndPosition;
                regions.Add(new Region(startPosition, length));
            }
            else if (node.Term.Name == NPLConstants.WhileBlock)
            {
                startPosition = node.ChildNodes[0].Span.EndPosition;
                length = node.ChildNodes[4].Span.Location.Position - node.ChildNodes[0].Span.EndPosition;
                regions.Add(new Region(startPosition, length));
            }
            else if (node.Term.Name == NPLConstants.RepeatBlock)
            {
                startPosition = node.ChildNodes[0].Span.EndPosition;
                length = node.ChildNodes[3].Span.EndPosition - node.ChildNodes[0].Span.EndPosition;
                regions.Add(new Region(startPosition, length));
            }
            else if (node.Term.Name == NPLConstants.ForBlock ||
                node.Term.Name == NPLConstants.GenericForBlock ||
                node.Term.Name == NPLConstants.ConditionBlock)
            {
                startPosition = node.ChildNodes[0].Span.EndPosition;
                length = node.ChildNodes[6].Span.Location.Position - node.ChildNodes[0].Span.EndPosition;
                regions.Add(new Region(startPosition, length));
            }

            foreach (ParseTreeNode child in node.ChildNodes)
            {
                WalkSyntaxTreeForOutliningRegions(child, regions);
            }
        }
    }


}
