using Irony.Interpreter.Ast;
using System;
using Irony.Ast;
using Irony.Parsing;

namespace NPLTools.IronyParser.Ast
{
    public class LuaBinaryExpressionNode : LuaNode
    {
        public LuaNode LeftExpression { get; private set; }
        public LuaNode RightExpression { get; private set; }
        public string Operator { get; private set; }
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            if (treeNode.ChildNodes.Count != 3) return;
            LeftExpression = treeNode.ChildNodes[0].AstNode as LuaNode;
            Operator = treeNode.ChildNodes[1].Token.Text;
            RightExpression = treeNode.ChildNodes[2].AstNode as LuaNode;

            AddChild("left expr", LeftExpression);
            AddChild("right expr", RightExpression);
        }
    }
}
