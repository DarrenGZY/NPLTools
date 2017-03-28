using System.Collections.Generic;

namespace NPLTools.Grammar.Ast
{
    /// <summary>
    /// Represents a block of statements in a Lua code file.
    /// </summary>
    public class Block : Node
    {
        public Node StatementList { get; set; }

    }
}
