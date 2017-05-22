using System;
using System.Collections.Generic;
using System.Diagnostics;
using Irony.Ast;
using Irony.Interpreter.Ast;
using Irony.Parsing;
using NPLTools.Intellisense;

namespace NPLTools.IronyParser.Ast
{
    //A node representing function definition
    public class LuaFunctionDefNode : LuaNode, IDeclaration
    {
        public LuaFuncIdentifierNode NameNode;
        public LuaIdentifierNodeList Parameters;
        public LuaBlockNode Body;
        public FunctionType Type;
        public string Description;

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            // child #0 is usually a keyword like "local" or "local function"
            // child #i is the keyword "end"
            Type = FunctionType.Declaration;
            if (treeNode.ChildNodes.Count == 4)
                Type = FunctionType.Anonymous;
            else if (treeNode.ChildNodes.Count == 5)
                Type = FunctionType.Declaration;
            else if (treeNode.ChildNodes.Count == 6)
                Type = FunctionType.LocalDeclaration;

            int i = treeNode.ChildNodes.Count-1;

            Debug.Assert(i>1);
            var anon = treeNode.ChildNodes[i - 3];
            
            if (anon != null && anon.Token != null && anon.Token.KeyTerm != null)
            {
                NameNode = new LuaFuncIdentifierNode().InitAnonymous();; 
            }
            else
            {
                NameNode = AddChild("Name", anon) as LuaFuncIdentifierNode;
            }
            var name = NameNode.AsString;

            if (treeNode.ChildNodes[i-2].ChildNodes.Count > 0 &&
                treeNode.ChildNodes[i-2].ChildNodes[0].AstNode is LuaIdentifierNodeList)
                Parameters = treeNode.ChildNodes[i - 2].ChildNodes[0].AstNode as LuaIdentifierNodeList;
            Body = AddChild("Body", treeNode.ChildNodes[i-1]) as LuaBlockNode;
            
            if (treeNode.Comments.Count > 0)
            {
                Description = treeNode.Comments[0].Text;
                for (int k = 1; k < treeNode.Comments.Count; ++k)
                {
                    Description += "\n" + treeNode.Comments[k].Text;
                }
            }

            AsString = "<Function " + name + ">";
        }

        public void GetDeclarations(LuaBlockNode block, LuaModel model)
        {
            // add parameter declaration
            if (Parameters != null && Body != null)
            {
                foreach (var identifier in Parameters.Identifiers)
                {
                    Body.Locals.Add(new Declaration(identifier.Identifier, String.Empty, model.FilePath,
                        new ScopeSpan(identifier.Span.EndPosition, identifier.EndLine, Body.Span.EndPosition, Body.EndLine)));
                }
            }

            // Anonymous Function
            if (Type == FunctionType.Anonymous)
                return;
            // function declaration
            else if (Type == FunctionType.Declaration)
            {
                if (NameNode.Namespaces == null)
                {
                    block.Globals.Add(new Declaration(NameNode.Name, Description, model.FilePath,
                        new ScopeSpan(NameNode.Span.EndPosition, NameNode.EndLine, int.MaxValue, int.MaxValue)));
                }
                else
                {
                    // if its namespace is in local scope, add it to local declaration list
                    foreach (var local in block.Locals)
                    {
                        if (local.Equal(BuildDeclaration(NameNode.Namespaces)))
                        {
                            block.Locals.Add(new Declaration(NameNode.Name, Description, model.FilePath,
                                new ScopeSpan(NameNode.Span.EndPosition, NameNode.EndLine, block.Span.EndPosition, block.EndLine), local));
                            break;
                        }
                    }
                    // if its namespace is in global scope, add it to global declaration list
                    foreach (var global in block.Globals)
                    {
                        if (global.Equal(BuildDeclaration(NameNode.Namespaces)))
                        {
                            block.Globals.Add(new Declaration(NameNode.Name, Description, model.FilePath,
                                new ScopeSpan(NameNode.Span.EndPosition, NameNode.EndLine, block.Span.EndPosition, block.EndLine), global));
                            break;
                        }
                    }
                }
            }
            // local function declaration
            else if (Type == FunctionType.LocalDeclaration)
            {
                Declaration declaration = new Declaration(NameNode.Name, Description, model.FilePath, 
                        new ScopeSpan(NameNode.Span.EndPosition, NameNode.EndLine, block.Span.EndPosition, block.EndLine));

                //foreach (var local in block.Locals)
                //{
                //    if (local.Equal(NameNode.Namespaces))
                //    {
                //        declaration.NameSpaces.Add(local);
                //        break;
                //    }
                //}
                block.Locals.Add(declaration);
            } 
        }

        private Declaration BuildDeclaration(string name)
        {
            int index = name.LastIndexOf('.');
            if (index == -1)
                return new Declaration(name);
            else
            {
                //string a = name.Substring(index+1);
                //string b = name.Substring(0, index);
                return new Declaration(name.Substring(index + 1), BuildDeclaration(name.Substring(0, index)));
            }
        }

        //#endregion
    }//class

    public enum FunctionType
    {
        Anonymous = 0,
        LocalDeclaration = 1,
        Declaration = 2
    }

}//namespace
