//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Irony.Parsing;
//using NPLTools.Grammar.Ast;
//namespace NPLTools.Tests
//{
//    [Language("My Sample", "1.0", "My Sample Programming Language")]
//    public class SampleGrammar : Irony.Parsing.Grammar
//    {
//        public SampleGrammar()
//        {
//            LanguageFlags |= LanguageFlags.CreateAst;
//            NumberLiteral number = new NumberLiteral("number", NumberOptions.Default, typeof(Node));

//            NonTerminal S = new NonTerminal("S", typeof(Node));
//            NonTerminal E = new NonTerminal("E", typeof(Node));

//            this.Root = S;
//            RegisterOperators(1, "+", "-");
//            RegisterOperators(2, "*", "/");
//            S.Rule = E;
//            E.Rule = E + "+" + E
//                | E + "-" + E
//                | E + "*" + E
//                | E + "/" + E
//                | number;
//        }
//    }
//}
