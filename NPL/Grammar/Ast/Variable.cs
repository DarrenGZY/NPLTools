using System.Collections.Generic;
using Irony.Ast;
using Irony.Parsing;

namespace NPLTools.Grammar.Ast
{
    /// <summary>
    /// Represents a variable node in the AST for the Lua code file.
    /// </summary>
    public class Variable : Node
    {
        public Node PrefixExpression { get; set; }

        public Node Expression { get; set; }

        public Identifier Identifier { get; set; }

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            this.Identifier = treeNode.ChildNodes[0].AstNode as Identifier;
        }
    }
}
