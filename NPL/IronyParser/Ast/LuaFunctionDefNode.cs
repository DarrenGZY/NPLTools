using System;
using System.Collections.Generic;
using System.Diagnostics;
using Irony.Ast;
using Irony.Interpreter.Ast;
using Irony.Parsing;

namespace NPLTools.IronyParser.Ast
{

    //A node representing function definition
    public class LuaFunctionDefNode : LuaNode, IDeclaration
    {
        public LuaFuncIdentifierNode NameNode;
        public LuaNode Parameters;
        public LuaNode Body;
        public FunctionType Type;

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

            Parameters = AddChild("Parameters", treeNode.ChildNodes[i-2]) as LuaNode;
            Body = AddChild("Body", treeNode.ChildNodes[i-1]) as LuaNode;
            
            AsString = "<Function " + name + ">";
        }

        public void GetDeclarations(LuaBlockNode block)
        {
            // Anonymous Function
            if (Type == FunctionType.Anonymous)
                return;
            // function declaration
            else if (Type == FunctionType.Declaration)
            {
                if (NameNode.Namespaces.Count == 0)
                {
                    block.Globals.Add(new Declaration(NameNode.Name,
                        new ScopeSpan(NameNode.Span.EndPosition, NameNode.EndLine, int.MaxValue, int.MaxValue)));
                }
                else
                {
                    foreach (var local in block.Locals)
                    {
                        if (local.NamesEqual(NameNode.Namespaces))
                        {
                            block.Locals.Add(new Declaration(NameNode.Name,
                                new ScopeSpan(NameNode.Span.EndPosition, NameNode.EndLine, block.Span.EndPosition, block.EndLine), new List<Declaration>() { local }));
                            break;
                        }
                    }
                }
            }
            // local function declaration
            else if (Type == FunctionType.LocalDeclaration)
            {
                Declaration declaration = new Declaration(NameNode.Name,
                        new ScopeSpan(NameNode.Span.EndPosition, NameNode.EndLine, block.Span.EndPosition, block.EndLine));

                foreach (var local in block.Locals)
                {
                    if (local.NamesEqual(NameNode.Namespaces))
                    {
                        declaration.NameSpaces.Add(local);
                        break;
                    }   
                }
                block.Locals.Add(declaration);
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
