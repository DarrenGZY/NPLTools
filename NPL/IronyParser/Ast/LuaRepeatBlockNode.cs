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
    public class LuaRepeatBlockNode : LuaNode
    {
        public AstNode Block { get; private set; }
        public AstNode Expression { get; private set; }

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            if (treeNode.ChildNodes.Count != 4) return;

            Block = treeNode.ChildNodes[1].AstNode as AstNode;
            Expression = treeNode.ChildNodes[3].AstNode as AstNode;

            AddChild("repeat block", Block);
            AddChild("repeat block expr", Expression);
        }
    }
}
