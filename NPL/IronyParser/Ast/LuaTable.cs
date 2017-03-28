using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Ast;
using Irony.Parsing;
using Irony.Interpreter.Ast;

namespace NPLTools.IronyParser.Ast
{
    public class LuaTable : AstNode
    {
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            if (treeNode.ChildNodes.Count > 0)
                AddChild("field list", treeNode.ChildNodes[0]);
            AsString = "LuaTable";
        }
    }
}
