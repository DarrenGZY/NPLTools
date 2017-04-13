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
        public LuaNode Block { get; private set; }
        public LuaNode Expression { get; private set; }

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            if (treeNode.ChildNodes.Count != 4) return;

            Block = treeNode.ChildNodes[1].AstNode as LuaNode;
            Expression = treeNode.ChildNodes[3].AstNode as LuaNode;

            AddChild("repeat block", Block);
            AddChild("repeat block expr", Expression);
        }
    }
}
