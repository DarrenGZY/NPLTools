using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Ast;
using Irony.Parsing;

namespace NPLTools.IronyParser.Ast
{
    public class LuaFieldAndSeperatorNode : LuaNode
    {
        public LuaField Field { get; set; }

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            if (treeNode.ChildNodes.Count == 0) return;
            Field = AddChild(String.Empty, treeNode.ChildNodes[0]) as LuaField;
        }
    }
}
