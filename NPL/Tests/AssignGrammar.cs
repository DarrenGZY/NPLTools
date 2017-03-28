//using Irony.Parsing;
//using NPLTools.Grammar.Ast;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace NPLTools.Tests
//{
//    public class AssignGrammar : Irony.Parsing.Grammar
//    {
//        public AssignGrammar()
//        {
//            LanguageFlags |= LanguageFlags.CreateAst;
//            NonTerminal Assign = new NonTerminal("assign", typeof(Assignment));
//            //NonTerminal VarList = new NonTerminal("varlist", typeof());
//            //NonTerminal Var = new NonTerminal("var");
//            //NonTerminal ExprList = new NonTerminal("exprlist");
//            NonTerminal Expr = new NonTerminal("expr", typeof(Literal));
//            IdentifierTerminal Name = new IdentifierTerminal("identifier");
//            Name.AstConfig.NodeType = typeof(Identifier);
//            KeyTerm EQ = new KeyTerm("=", "=");
//            var COMMA = ToTerm(",");
//            EQ.AstConfig.NodeType = typeof(Operator);
//            COMMA.AstConfig.NodeType = typeof(Operator);
//            var NIL = ToTerm("nil");
//            NIL.AstConfig.NodeType = typeof(Literal);
//            var FALSE = ToTerm("false");
//            FALSE.AstConfig.NodeType = typeof(Literal);
//            var TRUE = ToTerm("true");
//            TRUE.AstConfig.NodeType = typeof(Literal);

//            this.Root = Assign;
//            Assign.Rule = Name + EQ + Expr;
//            //VarList.Rule = MakePlusRule(VarList, COMMA, Name);
//            //ExprList.Rule = MakePlusRule(ExprList, COMMA, Expr);
//            Expr.Rule = NIL | FALSE | TRUE;
//        }
        
//    }
//}
