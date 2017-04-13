using Irony.Interpreter.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Ast;
using Irony.Parsing;

namespace NPLTools.IronyParser.Ast
{
    public class LuaWhileBlockNode : LuaNode
    {
        public LuaNode Expression;
        public LuaNode Block;

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            if (treeNode.ChildNodes.Count != 5) return;

            Expression = treeNode.ChildNodes[1].AstNode as LuaNode;
            Block = treeNode.ChildNodes[3].AstNode as LuaNode;

            AddChild("while loop expr", Expression);
            AddChild("while loop block", Block);
        }
    }
}
