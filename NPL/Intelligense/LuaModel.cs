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

        internal Dictionary<string, ScopeSpan> GetGlobals()
        {
            Dictionary<string, ScopeSpan> declarations = new Dictionary<string, ScopeSpan>();

            if (_root == null)
                return declarations;

            LuaBlockNode node = _root.ChildNodes[0] as LuaBlockNode;
            foreach (var declaration in node.Locals)
            {

                // TODO: some error here
                if (!declarations.ContainsKey(declaration.Name))
                    declarations.Add(declaration.Name, declaration.Scope);
            }

            return declarations;
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
