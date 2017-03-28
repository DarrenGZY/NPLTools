using System.Collections.Generic;

namespace NPLTools.Grammar.Ast
{
    /// <summary>
    /// Represents a function call statement node in the AST for the Lua code file.
    /// </summary>
    public class FunctionCall : Node
    {

        public Node PrefixExpression { get; set; }

        public Identifier Identifier { get; set; }

        public Node Arguments { get; set; }

        
    }
}
