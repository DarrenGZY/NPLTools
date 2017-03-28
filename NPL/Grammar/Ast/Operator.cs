using Irony.Interpreter.Ast;
using System;
using Irony.Ast;
using Irony.Parsing;

namespace NPLTools.Grammar.Ast
{
    public class Operator : Node
    {
        public string Op { get; set; }

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            
        }
    }
}
