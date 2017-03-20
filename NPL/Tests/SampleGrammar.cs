using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Parsing;
namespace NPL.Parser
{
    [Language("My Sample", "1.0", "My Sample Programming Language")]
    public class SampleGrammar : Irony.Parsing.Grammar
    {
        public SampleGrammar()
        {
            NumberLiteral number = new NumberLiteral("number");

            NonTerminal S = new NonTerminal("S");
            NonTerminal E = new NonTerminal("E");

            

            this.Root = S;
            RegisterOperators(1, "+", "-");
            RegisterOperators(2, "*", "/");
            S.Rule = E;
            E.Rule = E + "+" + E
                | E + "-" + E
                | E + "*" + E
                | E + "/" + E
                | number;
        }
    }
}
