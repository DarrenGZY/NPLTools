using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Ast;
using Irony.Parsing;

namespace NPLTools.IronyParser.Ast
{
    public class LuaTableAccessNode : LuaNode
    {
        public LuaNode PrefixNode;
        public LuaNode Expr;

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            // prefixexp `.´ Name 
            if (treeNode.ChildNodes.Count == 3)
            {
                AsString = (treeNode.ChildNodes[0].AstNode as LuaNode) + "." + (treeNode.ChildNodes[2].AstNode as LuaNode);
                PrefixNode = AddChild(String.Empty, treeNode.ChildNodes[0]) as LuaNode;
                Expr = AddChild(String.Empty, treeNode.ChildNodes[2]) as LuaNode;
            }

            // prefixexp `[´ exp `]´ 
            if (treeNode.ChildNodes.Count == 4)
            {

            }
        }
    }
}
