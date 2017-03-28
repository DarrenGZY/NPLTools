using System;

namespace NPLTools.Grammar.Ast
{
    public class Function : Node
    {
        private string name = "~Function_" + Guid.NewGuid();

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public bool IsScope
        {
            get { return true; }
        }

        public ParameterList ParameterList { get; set; }

        public Block Body { get; set; }

	}
}
