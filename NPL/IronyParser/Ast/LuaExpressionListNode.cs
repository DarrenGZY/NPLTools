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
    public class LuaExpressionNodeList : AstNode
    {
        public List<AstNode> ExpressionList { get; set; }

        public LuaExpressionNodeList()
        {
            ExpressionList = new List<AstNode>();
        }

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            foreach (var child in treeNode.ChildNodes)
            {
                ExpressionList.Add(AddChild("expr", child));
            }

            AsString = "expression list";
        }
    }
}
