using System.Collections.Generic;

namespace NPLTools.Grammar.Ast
{
    /// <remarks>
    /// The field can represent the following Lua language constructs:
    /// <code>[LeftExpression] = Expression</code>
    /// <code>Identifier = Expression</code>
    /// <code>Expression</code>
    /// </remarks>
    public class Field : Node
    {
        public Identifier Identifier { get; set; }

        public Node Expression { get; set; }

        public Node LeftExpression { get; set; }
        

	}
}
