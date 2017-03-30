using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Ast;
using Irony.Parsing;
using Irony.Interpreter.Ast;

namespace NPLTools.IronyParser.Ast
{
    public class LuaDoBlockNode : LuaNode
    {
        public LuaNode Block { get; private set; }

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            Block = treeNode.ChildNodes[1].AstNode as AstNode;
            AddChild("do block", Block);
        }
    }
}
