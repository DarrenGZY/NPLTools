﻿using Irony.Interpreter.Ast;
using Irony.Parsing;
using NPLTools.IronyParser;
using NPLTools.IronyParser.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SourceSpan = NPLTools.Intellisense.SourceSpan;

namespace NPLTools.Intellisense
{
    public class LuaModel
    {
        private LuaChunkNode _root;
        private ParseTree _parseTree;
        private AnalysisEntry _entry;
        private string _filePath;
        public Dictionary<string, LuaModel> IncludedFiles = new Dictionary<string, LuaModel>();

        public string FilePath => _filePath;

        public AnalysisEntry Entry => _entry;

        public LuaModel(ParseTree parseTree, AnalysisEntry entry)
        {
            _parseTree = parseTree;
            _entry = entry;
            _filePath = entry.FilePath;
            if (_parseTree.Root!= null)
            {
                _root = _parseTree.Root.AstNode as LuaChunkNode;
                WalkASTForDeclarations();
            }
        }

        public LuaModel(ParseTree parseTree, string filePath)
        {
            _parseTree = parseTree;
            _entry = null;
            _filePath = filePath;
            if (_parseTree.Root != null)
            {
                _root = _parseTree.Root.AstNode as LuaChunkNode;
                WalkASTForDeclarations();
            }
        }

        public void Update(ParseTree parseTree)
        {
            _parseTree = parseTree;

            if (_parseTree.Root != null && _parseTree.Root.AstNode != null)
            {
                _root = _parseTree.Root.AstNode as LuaChunkNode;
                WalkASTForDeclarations();
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
        /// Get global declaration in the current lua model
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Declaration GetGlobalDeclaration(string name)
        {
            if (_root == null)
                return null;
            List<Declaration> foundedDeclarations = new List<Declaration>();
            // build a declaration with dummy scope span
            Declaration declaration = DeclarationHelper.BuildDeclaration(name);
            GetGlobalDeclarationsByName(_root, declaration, foundedDeclarations);
            foundedDeclarations.Sort(delegate (Declaration a, Declaration b)
            {
                if (a.Scope.StartPosition >= b.Scope.StartPosition && 
                    a.Scope.EndPosition <= b.Scope.EndPosition)
                    return -1;
                else
                    return 1;
            });

            if (foundedDeclarations.Count > 0)
                return foundedDeclarations[0];

            return null;
        }

        public void WalkASTForCompletionSource(int triggerPosition, HashSet<Declaration> res)
        {
            WalkASTForCompletionSource(_root, triggerPosition, res);
        }

        private void WalkASTForCompletionSource(LuaNode node, int triggerPosition, HashSet<Declaration> res)
        {
            if (node == null)
                return;

            if (node is LuaBlockNode)
            {
                if (node.Parent is LuaChunkNode)
                {
                    foreach (var declaration in ((LuaBlockNode)node).Locals)
                    {
                        if (triggerPosition > declaration.Scope.StartPosition)
                            res.Add(declaration);
                    }
                }
                else
                {
                    foreach (var declaration in ((LuaBlockNode)node).Locals)
                    {
                        if (triggerPosition > declaration.Scope.StartPosition &&
                            triggerPosition < declaration.Scope.EndPosition)
                            res.Add(declaration);
                    }
                }

                foreach (var declaration in ((LuaBlockNode)node).Globals)
                {
                    if (triggerPosition > declaration.Scope.StartPosition)
                        res.Add(declaration);
                }

                foreach (var declaration in ((LuaBlockNode)node).Requires)
                {
                        res.Add(declaration);
                }
            }
            foreach (AstNode child in node.ChildNodes)
            {
                if (!(child is NullNode))
                    WalkASTForCompletionSource(child as LuaNode, triggerPosition, res);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="span"></param>
        /// <returns></returns>
        public Declaration GetDeclaration(string name, ScopeSpan span)
        {
            if (_root == null)
                return null;
            List<Declaration> foundDeclarations = new List<Declaration>();
            Declaration declaration = DeclarationHelper.BuildDeclaration(name, span);
            GetDeclarationsByName(_root, declaration, foundDeclarations);
            foundDeclarations.Sort(delegate (Declaration a, Declaration b)
            {
                if (a.Scope.StartPosition >= b.Scope.StartPosition && 
                    a.Scope.EndPosition <= b.Scope.EndPosition)
                    return -1;
                else
                    return 1;
            });

            if (foundDeclarations.Count > 0)
                return foundDeclarations[0];

            return null;
        }

        private void GetDeclarationsByName(LuaNode node, Declaration declaration, List<Declaration> foundDeclarations)
        {
            if (node is LuaBlockNode)
            {
                //node = node as LuaBlockNode;
                foreach (var local in ((LuaBlockNode)node).Locals)
                {
                    if (local.Equal(declaration))
                    {
                        if (declaration.Scope.StartPosition >= local.Scope.StartPosition &&
                            declaration.Scope.EndPosition <= local.Scope.EndPosition)
                            foundDeclarations.Add(local);
                        else if (declaration.Scope.EndPosition == local.Scope.StartPosition)
                            foundDeclarations.Add(local);
                    }
                }

                foreach (var global in ((LuaBlockNode)node).Globals)
                {
                    if (global.Equal(declaration))
                    {
                        if (declaration.Scope.StartPosition >= global.Scope.StartPosition &&
                            declaration.Scope.EndPosition <= global.Scope.EndPosition)
                            foundDeclarations.Add(global);
                        else if (declaration.Scope.EndPosition == global.Scope.StartPosition)
                            foundDeclarations.Add(global);
                    }
                }
            }
            foreach (AstNode child in node.ChildNodes)
            {
                if (!(child is NullNode))
                    GetDeclarationsByName(child as LuaNode, declaration, foundDeclarations);
            }
        }

        // Get global declarations from files in project other than the current file
        //public IEnumerable<Declaration> GetGlobalDeclarationInProject()
        //{
        //    var entries = _entry.Analyzer.GetAnalysisEntries();
        //    var validEntries = entries.Where((entry) => entry.FilePath != _entry.FilePath && entry.Model != null);
        //    List<Declaration> res = new List<Declaration>();
        //    for (int i = 0; i < validEntries.Count(); ++i)
        //    {
        //        res.Concat(validEntries.ElementAt(i).Model.GetGlobalDeclarations());
        //    }
        //    return res;
        //}

        private void GetGlobalDeclarationsByName(LuaNode node, Declaration declaration, List<Declaration> foundedDeclarations)
        {
            if (node is LuaBlockNode)
            {
                foreach (var global in ((LuaBlockNode)node).Globals)
                {
                    if (global.Equal(declaration))
                    {
                        foundedDeclarations.Add(global);
                    }
                }
            }
            foreach (AstNode child in node.ChildNodes)
            {
                if (!(child is NullNode))
                    GetGlobalDeclarationsByName(child as LuaNode, declaration, foundedDeclarations);
            }  
        }

        public IEnumerable<Declaration> GetGlobalDeclarations()
        {
            return RetreiveGlobalDeclarationsFromAST(_root);
        }

        private IEnumerable<Declaration> RetreiveGlobalDeclarationsFromAST(LuaNode node)
        {
            if (node is LuaBlockNode)
            {
                foreach (var global in ((LuaBlockNode)node).Globals)
                {
                    yield return global;
                }
            }
            foreach (AstNode child in node.ChildNodes)
            {
                if (!(child is NullNode))
                    foreach (var declaration in RetreiveGlobalDeclarationsFromAST(child as LuaNode))
                        yield return declaration;
            }
        }

        private void WalkASTForDeclarations()
        {
            ClearIncludedFiles();
            WalkASTForDeclarations(_root);
        }

        private void WalkASTForDeclarations(LuaNode node)
        {
            if (node is LuaBlockNode)
            {
                foreach (AstNode child in node.ChildNodes)
                {
                    if (child is IDeclaration)
                    {
                        ((IDeclaration)child).GetDeclarations((node as LuaBlockNode), this);
                    }
                }
            }
            foreach (AstNode child in node.ChildNodes)
            {
                if (!(child is NullNode))
                    WalkASTForDeclarations(child as LuaNode);
            }
        }

        public void AddIncludedFile(string path, LuaModel model)
        {
            if (IncludedFiles.ContainsKey(path))
                IncludedFiles[path] = model;
            else
                IncludedFiles.Add(path, model);
        }

        public void ClearIncludedFiles()
        {
            IncludedFiles.Clear();
        }

        #region Format Helpers
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
            foreach (AstNode childNode in node.ChildNodes)
            {
                if (!(childNode is NullNode))
                    WalkSyntaxTreeForIndentations(childNode as LuaNode, indentations);
            }
        }
        #endregion

        #region Outlining Helpers
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
        #endregion
    }


}
