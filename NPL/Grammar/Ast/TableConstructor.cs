using System;
using System.Collections.Generic;

namespace NPLTools.Grammar.Ast
{
    public class TableConstructor : Node
    {
        // Initialize a default name for the scope if no other name is assigned.
        private string name = "~Table_" + Guid.NewGuid();

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public Field FieldList { get; set; }

    }
}
