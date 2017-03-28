using System;
using Irony.Ast;
using Irony.Parsing;

namespace NPLTools.Grammar.Ast
{
    /// <summary>
    /// Represents an identifier node in the AST for the Lua code file.
    /// </summary>
    public class Identifier : Node
    {
        public string Name { get; set; }

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            this.Name = treeNode.Token.Text;
        }
    }
}
