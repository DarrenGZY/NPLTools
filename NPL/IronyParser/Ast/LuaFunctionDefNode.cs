﻿using System;
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
        LuaNode NameNode;
        LuaNode Parameters;
        LuaNode Body;

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            // child #0 is usually a keyword like "local" or "local function"
            // child #i is the keyword "end"
            int i = treeNode.ChildNodes.Count-1;

            Debug.Assert(i>1);
            var anon = treeNode.ChildNodes[i - 3];
            
            if (anon != null && anon.Token != null && anon.Token.KeyTerm != null)
            {
                NameNode = new LuaFuncIdentifierNode().InitAnonymous();; 
            }
            else
            {
                NameNode = AddChild("Name", anon) as LuaNode;
            }
            var name = NameNode.AsString;

            Parameters = AddChild("Parameters", treeNode.ChildNodes[i-2]) as LuaNode;
            Body = AddChild("Body", treeNode.ChildNodes[i-1]) as LuaNode;
            
            AsString = "<Function " + name + ">";
        }

        public void GetDeclarations(LuaBlockNode block)
        {
            block.Locals.Add(NameNode);
        }

        //#endregion
    }//class

}//namespace
