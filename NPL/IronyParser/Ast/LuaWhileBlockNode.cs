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
        public AstNode Expression;
        public AstNode Block;

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            if (treeNode.ChildNodes.Count != 5) return;

            Expression = treeNode.ChildNodes[1].AstNode as AstNode;
            Block = treeNode.ChildNodes[3].AstNode as AstNode;

            AddChild("while loop expr", Expression);
            AddChild("while loop block", Block);
        }
    }
}
