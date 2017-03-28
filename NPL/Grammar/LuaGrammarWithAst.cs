﻿using System;
using Irony.Parsing;
using NPLTools.Grammar.Ast;

namespace NPLTools.Grammar
{
    [Language("Lua", "5.1", "Lua Script Language")]
    public class LuaGrammarWithAst : Irony.Parsing.Grammar
    {
        public static LuaGrammarWithAst Instance
        {
            get
            {
                if (CurrentGrammar == null) return new LuaGrammarWithAst();
                return (LuaGrammarWithAst)CurrentGrammar;
            }
        }

        private LuaGrammarWithAst() :
            base(true)
        {
            // Set Create Ast flag, enable creating ast while doing parsing
            LanguageFlags |= LanguageFlags.CreateAst;
            
            #region Declare Terminals Here
            StringLiteral STRING = CreateLuaString(LuaTerminalNames.String);
            NumberLiteral NUMBER = CreateLuaNumber(LuaTerminalNames.Number);

            LuaLongStringTerminal LONGSTRING = new LuaLongStringTerminal(LuaTerminalNames.LongString);

            // This includes both single-line and block comments
            var Comment = new LuaCommentTerminal(LuaTerminalNames.Comment);

            //  Regular Operators
            var DOT = ToTerm(LuaTerminalNames.Dot);
            var COLON = ToTerm(LuaTerminalNames.Colon);
            var SEMIC = ToTerm(LuaTerminalNames.Semic);
            var COMMA = ToTerm(LuaTerminalNames.Comma);

            //  Standard Operators
            var EQ = Operator(LuaTerminalNames.Equal);

            var OpeAdd = Operator("+");
            var OpeSub = Operator("-");
            var OpeMul = Operator("*");
            var OpeDiv = Operator("/");
            var OpePow = Operator("^");

            var OpeEQ = Operator("==");
            var OpeNEQ = Operator("~=");
            var OpeBig = Operator(">");
            var OpeBigEQ = Operator(">=");
            var OpeSm = Operator("<");
            var OpeSmEQ = Operator("<=");

            var OpeLink = Operator("..");

            var OpePre = Operator("%");
            var OpeWell = Operator("#");

            NonGrammarTerminals.Add(Comment);

            #region Keywords
            var LOCAL = Keyword("local");
            var DO = Keyword("do");
            var END = Keyword("end");
            var WHILE = Keyword("while");
            var REPEAT = Keyword("repeat");
            var UNTIL = Keyword("until");
            var IF = Keyword("if");
            var THEN = Keyword("then");
            var ELSEIF = Keyword("elseif");
            var ELSE = Keyword("else");
            var FOR = Keyword("for");
            var IN = Keyword("in");
            var FUNCTION = Keyword("function");
            var RETURN = Keyword("return");
            var BREAK = Keyword("break");
            var NIL = Keyword("nil");
            var FALSE = Keyword("false");
            var TRUE = Keyword("true");
            var NOT = Keyword("not");
            var AND = Keyword("and");
            var OR = Keyword("or");

            //var CLASS = Keyword("class");
            //var NEW = Keyword("new");
            #endregion
            IdentifierTerminal Name = new IdentifierTerminal(LuaTerminalNames.Identifier);
            Name.AstConfig.NodeType = typeof(Identifier);

            #endregion

            #region Declare NonTerminals Here
            NonTerminal Chunk = new NonTerminal(LuaTerminalNames.Chunk, typeof(Chunk));
            NonTerminal Block = new NonTerminal(LuaTerminalNames.Block, typeof(Block));

            NonTerminal Statement = new NonTerminal(LuaTerminalNames.Statement, typeof(Statement));
            NonTerminal LastStatement = new NonTerminal(LuaTerminalNames.LastStatement, typeof(LastStatement));
            NonTerminal FuncName = new NonTerminal(LuaTerminalNames.FunctionName, typeof(FunctionName));
            NonTerminal VarList = new NonTerminal(LuaTerminalNames.VarList, typeof(VariableList));
            NonTerminal Var = new NonTerminal(LuaTerminalNames.Var, typeof(Variable));
            NonTerminal NameList = new NonTerminal(LuaTerminalNames.NameList, typeof(NameList));
            NonTerminal ExprList = new NonTerminal(LuaTerminalNames.ExprList, typeof(Expression));
            NonTerminal Expr = new NonTerminal(LuaTerminalNames.Expr, typeof(Expression));
            NonTerminal PrefixExpr = new NonTerminal(LuaTerminalNames.PrefixExpr, typeof(Expression));
            NonTerminal FunctionCall = new NonTerminal(LuaTerminalNames.FunctionCall, typeof(FunctionCall));
            NonTerminal Args = new NonTerminal(LuaTerminalNames.Args, typeof(Arguments));
            NonTerminal NamedFunction = new NonTerminal(LuaTerminalNames.NamedFunction, typeof(Function));
            NonTerminal NamelessFunction = new NonTerminal(LuaTerminalNames.NamelessFunction, typeof(Function));
            NonTerminal FuncBody = new NonTerminal(LuaTerminalNames.FunctionBody, typeof(FunctionBody));
            NonTerminal ParList = new NonTerminal(LuaTerminalNames.ParList, typeof(ParameterList));
            NonTerminal TableConstructor = new NonTerminal(LuaTerminalNames.TableConstructor, typeof(TableConstructor));
            NonTerminal FieldList = new NonTerminal(LuaTerminalNames.FieldList, typeof(Field));
            NonTerminal Field = new NonTerminal(LuaTerminalNames.Field, typeof(Field));
            NonTerminal FieldSep = new NonTerminal(LuaTerminalNames.FieldSeperator, typeof(Field));
            NonTerminal BinOp = new NonTerminal(LuaTerminalNames.Binop, typeof(Node));
            NonTerminal UnOp = new NonTerminal(LuaTerminalNames.Unop, typeof(Node));

            NonTerminal LoopBlock = new NonTerminal(LuaTerminalNames.LoopBlock, typeof(Node));
            NonTerminal BranchBlock = new NonTerminal(LuaTerminalNames.BranchBlock, typeof(Node));
            NonTerminal Assign = new NonTerminal(LuaTerminalNames.Assign, typeof(Node));
            NonTerminal LocalVar = new NonTerminal(LuaTerminalNames.LocalVar, typeof(Node));

            #endregion

            #region Place Rules Here
            //Using Lua 5.1 grammar as defined in
            //http://www.lua.org/manual/5.1/manual.html#8
            this.Root = Chunk;

            //chunk ::= {stat [`;´]} [laststat [`;´]]
            Chunk.Rule = MakeStarRule(Chunk, Statement + SEMIC.Q()) + (LastStatement + SEMIC.Q()).Q();

            //block ::= chunk
            Block = Chunk;

            //stat ::=  varlist `=´ explist | 
            //     functioncall | 
            //     do block end | 
            //     while exp do block end | 
            //     repeat block until exp | 
            //     if exp then block {elseif exp then block} [else block] end | 
            //     for Name `=´ exp `,´ exp [`,´ exp] do block end | 
            //     for namelist in explist do block end | 
            //     function funcname funcbody | 
            //     local function Name funcbody | 
            //     local namelist [`=´ explist] 

            Statement.Rule = Assign |
                            FunctionCall |
                            DO + Block + END |
                            LoopBlock |
                            BranchBlock |
                            NamedFunction |
                            LocalVar
                            ;
            Assign.Rule = VarList + EQ + ExprList;
            LoopBlock.Rule = WHILE + Expr + DO + Block + END |
                            REPEAT + Block + UNTIL + Expr |
                            FOR + Name + EQ + Expr + COMMA + Expr + (COMMA + Expr).Q() + DO + Block + END |
                            FOR + NameList + IN + ExprList + DO + Block + END;
            BranchBlock.Rule = IF + Expr + THEN + Block + MakeStarRule(BranchBlock, ELSEIF + Expr + THEN + Block) + (ELSE + Block).Q() + END;
            LocalVar.Rule = LOCAL + NamedFunction |
                            LOCAL + NameList + (EQ + ExprList).Q();

            //laststat ::= return [explist] | break
            LastStatement.Rule = RETURN + ExprList.Q() | BREAK;

            //funcname ::= Name {`.´ Name} [`:´ Name]
            FuncName.Rule = Name + MakeStarRule(DOT + Name) + (COLON + Name).Q();

            //NamedFunction = 'function' + FuncName + FuncBody
            NamedFunction.Rule = FUNCTION + FuncName + FuncBody;

            //varlist ::= var {`,´ var}
            VarList.Rule = MakePlusRule(VarList, COMMA, Var);

            //namelist ::= Name {`,´ Name}
            NameList.Rule = MakePlusRule(NameList, COMMA, Name);

            //explist ::= {exp `,´} exp
            ExprList.Rule = MakePlusRule(ExprList, COMMA, Expr);

            //exp ::=  nil | false | true | Number | String | `...´ | function | 
            //     prefixexp | tableconstructor | exp binop exp | unop exp 
            Expr.Rule = NIL | FALSE | TRUE | NUMBER | STRING | LONGSTRING | "..." | NamelessFunction |
                PrefixExpr | TableConstructor | Expr + BinOp + Expr | UnOp + Expr;

            //var ::=  Name | prefixexp `[´ exp `]´ | prefixexp `.´ Name 
            Var.Rule = Name | PrefixExpr + "[" + Expr + "]" | PrefixExpr + DOT + Name;

            //prefixexp ::= var | functioncall | `(´ exp `)´
            PrefixExpr.Rule = Var | FunctionCall | "(" + Expr + ")";

            //functioncall ::=  prefixexp args | prefixexp `:´ Name args 
            FunctionCall.Rule = PrefixExpr + Args | PrefixExpr + COLON + Name + Args;

            //args ::=  `(´ [explist] `)´ | tableconstructor | String 
            Args.Rule = "(" + ExprList.Q() + ")" | TableConstructor | STRING | LONGSTRING;

            //function ::= function funcbody
            NamelessFunction.Rule = FUNCTION + FuncBody;

            //funcbody ::= `(´ [parlist] `)´ block end
            FuncBody.Rule = "(" + ParList.Q() + ")" + Block + END;

            //parlist ::= namelist [`,´ `...´] | `...´
            ParList.Rule = NameList + (COMMA + "...").Q() | "...";

            //tableconstructor ::= `{´ [fieldlist] `}´
            TableConstructor.Rule = "{" + FieldList.Q() + "}";

            //fieldlist ::= field {fieldsep field} [fieldsep]
            //FieldList.Rule = Field + MakeStarRule(FieldSep + Field) + FieldSep.Q();
            FieldList.Rule = MakePlusRule(FieldList, Field + FieldSep.Q());

            //field ::= `[´ exp `]´ `=´ exp | Name `=´ exp | exp
            Field.Rule = "[" + Expr + "]" + EQ + Expr | Name + EQ + Expr | Expr;

            //fieldsep ::= `,´ | `;´
            FieldSep.Rule = COMMA | SEMIC;

            //binop ::= `+´ | `-´ | `*´ | `/´ | `^´ | `%´ | `..´ | 
            //     `<´ | `<=´ | `>´ | `>=´ | `==´ | `~=´ | 
            //     and | or
            BinOp.Rule = OpeAdd | OpeSub | OpeMul | OpeDiv | OpePow | OpePre | OpeLink |
                    OpeSm | OpeSmEQ | OpeBig | OpeBigEQ | OpeEQ | OpeNEQ |
                    AND | OR;

            //unop ::= `-´ | not | `#´
            UnOp.Rule = NOT | OpeSub | OpeWell;

            #endregion

            #region Define Keywords and Register Symbols
            this.RegisterBracePair("(", ")");
            this.RegisterBracePair("{", "}");
            this.RegisterBracePair("[", "]");

            this.MarkPunctuation(",", ";");

            this.RegisterOperators(1, OR);
            this.RegisterOperators(2, AND);
            this.RegisterOperators(3, OpeBigEQ, OpeBig, OpeSm, OpeSmEQ, OpeNEQ, OpeEQ);
            this.RegisterOperators(4, Associativity.Right, OpeLink);
            this.RegisterOperators(5, OpeAdd, OpeSub);
            this.RegisterOperators(6, OpeMul, OpeDiv, OpePre);
            this.RegisterOperators(7, NOT);
            this.RegisterOperators(8, Associativity.Right, OpePow);

            #endregion
        }

        BnfExpression MakeStarRule(BnfTerm term)
        {
            return MakeStarRule(new NonTerminal(term.Name + "*"), term);
        }

        //Must create new overrides here in order to support the "Operator" token color
        public new void RegisterOperators(int precedence, params string[] opSymbols)
        {
            RegisterOperators(precedence, Associativity.Left, opSymbols);
        }

        //Must create new overrides here in order to support the "Operator" token color
        public new void RegisterOperators(int precedence, Associativity associativity, params string[] opSymbols)
        {
            foreach (string op in opSymbols)
            {
                KeyTerm opSymbol = Operator(op);
                opSymbol.Precedence = precedence;
                opSymbol.Associativity = associativity;
            }
        }

        public KeyTerm Keyword(string keyword)
        {
            var term = ToTerm(keyword);
            // term.SetOption(TermOptions.IsKeyword, true);
            // term.SetOption(TermOptions.IsReservedWord, true);

            this.MarkReservedWords(keyword);
            term.EditorInfo = new TokenEditorInfo(TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            term.AstConfig.NodeType = typeof(Keyword);
            return term;
        }

        public KeyTerm Operator(string op)
        {
            string opCased = this.CaseSensitive ? op : op.ToLower();
            var term = new KeyTerm(opCased, op);
            term.EditorInfo = new TokenEditorInfo(TokenType.Operator, TokenColor.Keyword, TokenTriggers.None);
            term.AstConfig.NodeType = typeof(Operator);
            return term;
        }

        protected static NumberLiteral CreateLuaNumber(string name)
        {
            NumberLiteral term = new NumberLiteral(name, NumberOptions.AllowStartEndDot);
            //default int types are Integer (32bit) -> LongInteger (BigInt); Try Int64 before BigInt: Better performance?
            term.DefaultIntTypes = new TypeCode[] { TypeCode.Int32, TypeCode.Int64, NumberLiteral.TypeCodeBigInt };
            term.DefaultFloatType = TypeCode.Double; // it is default
            term.AddPrefix("0x", NumberOptions.Hex);
            term.AstConfig.NodeType = typeof(Literal);
            return term;
        }

        protected static StringLiteral CreateLuaString(string name)
        {
            StringLiteral term = new LuaStringLiteral(name);
            term.AstConfig.NodeType = typeof(Literal);
            return term;
        }
    }
}
