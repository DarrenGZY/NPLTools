using System;
using Irony.Ast;
using Irony.Parsing;

namespace NPLTools.Grammar.Ast
{
    public class Literal : Node
    {
        // public LuaType Type { get; set; }

        public string Value { get; set; }

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            //this.Value = treeNode.Token.Text;
        }
    }
}
