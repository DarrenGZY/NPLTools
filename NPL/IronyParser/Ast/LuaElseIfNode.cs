using NPLTools.IronyParser.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Ast;
using Irony.Parsing;

namespace NPLTools.IronyParser.Ast
{
    public class LuaElseIfNode : LuaNode
    {
        LuaNode Expression;
        LuaBlockNode ElseIfBlock;

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            if (treeNode.ChildNodes.Count != 4) return;
            Expression = AddChild("elseif expr", treeNode.ChildNodes[1]) as LuaNode;
            ElseIfBlock = AddChild("elseif block", treeNode.ChildNodes[3]) as LuaBlockNode;
        }
    }
}
