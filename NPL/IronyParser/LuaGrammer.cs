using System;
using Irony.Ast;
//using Irony.Lua.Ast;
using NPLTools.IronyParser.Ast;
using NPLTools.IronyParser.Parser;
using Irony.Parsing;

// ReSharper disable InconsistentNaming

namespace NPLTools.IronyParser
{
    [Language("Lua", "5.1", "Lua Script Language")]
    public class LuaGrammar : Irony.Parsing.Grammar
    {
        private static LuaGrammar _instance;
        public static LuaGrammar Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new LuaGrammar();
                return _instance;
            }
        }

        public LuaGrammar() :
            base(true)
        {
            #region Declare Terminals Here

            StringLiteral STRING = CreateLuaString("string");
            NumberLiteral NUMBER = CreateLuaNumber("number");

            var LONGSTRING = new LuaLongStringTerminal("long-string");

            // This includes both single-line and block comments
            var Comment = new LuaCommentTerminal("block-comment");

            //  Regular Operators

            //  Member Select Operators
            KeyTerm DOT = Operator(".");
            DOT.EditorInfo = new TokenEditorInfo(TokenType.Operator, TokenColor.Text, TokenTriggers.MemberSelect);

            KeyTerm COLON = Operator(":");
            COLON.EditorInfo = new TokenEditorInfo(TokenType.Operator, TokenColor.Text, TokenTriggers.MemberSelect);

            //  Standard Operators
            KeyTerm EQ = Operator("=");

            KeyTerm MINUS = Operator("-");
            KeyTerm PLUS = Operator("+");
            KeyTerm UMINUS = Operator("-");
            KeyTerm CONCAT = Operator("..");
            KeyTerm GETN = Operator("#");
            KeyTerm NOT = Keyword("not");
            KeyTerm AND = Keyword("and");
            KeyTerm OR = Keyword("or");
            NonGrammarTerminals.Add(Comment);

            #region Keywords

            KeyTerm LOCAL = Keyword("local");
            KeyTerm DO = Keyword("do");
            KeyTerm END = Keyword("end");
            KeyTerm WHILE = Keyword("while");
            KeyTerm REPEAT = Keyword("repeat");
            KeyTerm UNTIL = Keyword("until");
            KeyTerm IF = Keyword("if");
            KeyTerm THEN = Keyword("then");
            KeyTerm ELSEIF = Keyword("elseif");
            KeyTerm ELSE = Keyword("else");
            KeyTerm FOR = Keyword("for");
            KeyTerm IN = Keyword("in");
            KeyTerm FUNCTION = Keyword("function");
            KeyTerm RETURN = Keyword("return");
            KeyTerm BREAK = Keyword("break");
            KeyTerm NIL = LiteralKeyword("nil");
            KeyTerm FALSE = LiteralKeyword("false");
            KeyTerm TRUE = LiteralKeyword("true");
            //var NIL = new ConstantTerminal("nil", typeof(LuaLiteralNode));
            //var FALSE = new ConstantTerminal("false", typeof(LuaLiteralNode));
            //var TRUE = new ConstantTerminal("true", typeof(LuaLiteralNode));

            KeyTerm ELLIPSIS = Keyword("...");

            #endregion

            var Name = new IdentifierTerminal("identifier");
            Name.AstConfig.NodeType = typeof(LuaIdentifierNode);
            #endregion

            #region Declare Transient NonTerminals Here

            // Organize the transient non-terminals so they are easier to find. None of these
            // will have an AST type by definition

            var Statement = new NonTerminal("statement");
            var LastStatement = new NonTerminal("last statement");
            var Statements = new NonTerminal("statement list");
            var StatementsEnd = new NonTerminal("statement list end");
            var SingleStatementWithTermOpt = new NonTerminal("single statement", Statement | Statement + ";");
            //SingleStatementWithTermOpt.AstConfig.NodeType = typeof(LuaBlockNode);
            var LastStatementWithTermOpt = new NonTerminal("last statement", LastStatement | LastStatement + ";");
            //LastStatementWithTermOpt.AstConfig.NodeType = typeof(LuaBlockNode);

            var BinOp = new NonTerminal("binop");
            var UnOp = new NonTerminal("unop");

            var Expr = new NonTerminal("expr");
            var PrefixExpr = new NonTerminal("prefix expr");
            var Var = new NonTerminal("var");
            var Args = new NonTerminal("args");
            var EllipsisOpt = new NonTerminal("optional ellipsis");

            var ParentheticalExpression = new NonTerminal("parentheical expression");
            var ArgsParameters = new NonTerminal("parentheical arguments");

            var ColonCallOpt = new NonTerminal("colon call");
            ColonCallOpt.AstConfig.NodeType = typeof(LuaBlockNode);
            var FunctionParameters = new NonTerminal("function parameters");

            
            MarkTransient(Expr, Statement, Statements, StatementsEnd, SingleStatementWithTermOpt, /* ColonCallOpt, */
                          FunctionParameters,
                          LastStatementWithTermOpt, UnOp, BinOp, PrefixExpr, Var, Args, EllipsisOpt, ArgsParameters,
                          ParentheticalExpression);

            #endregion

            #region Declare NonTerminals Here

            // These non-terminals will all require AST types. Anything that isnt really a language construct should be
            // refactored into a transient

            var Chunk = new NonTerminal("chunk", typeof (LuaChunkNode));
            var Block = new NonTerminal("block", typeof (LuaBlockNode)); // probably should be transient

            var FuncName = new NonTerminal("function name", typeof (LuaFuncIdentifierNode));
            var VarList = new NonTerminal("var list", typeof (LuaIdentifierNodeList));
            var NameList = new NonTerminal("name list", typeof (LuaIdentifierNodeList));
            var ExprList = new NonTerminal("expr list", typeof (LuaExpressionNodeList));
            var ExprListOpt = new NonTerminal("expr list opt", typeof(LuaExpressionNodeList));

            var FunctionCall = new NonTerminal("function call", typeof (LuaFunctionCallNode));
            var Function = new NonTerminal("anonymous function definition", typeof (LuaFunctionDefNode));
            //var FuncBody = new NonTerminal("function body", typeof (LuaBlockNode));
            var ParList = new NonTerminal("parlist", typeof (LuaParmListNode));

            var LocalVariableDeclaration = new NonTerminal("local variable declaration", typeof(LuaLocalDeclaration));
            var LocalVariableDeclarationWithAssignment = new NonTerminal("local variable declaration with assignment", typeof(LuaLocalDeclarationAssignment));
            var TableConstructor = new NonTerminal("table constructor", typeof(LuaTableNode));
            var FieldList = new NonTerminal("field list", typeof(LuaExpressionNodeList));
            var Field = new NonTerminal("field", typeof(LuaField));
            var FieldSep = new NonTerminal("field seperator", typeof(LuaNode));

            var IdentifierWithOptNamespace = new NonTerminal("identifier including namespace", typeof(LuaNode));

            var BinExp = new NonTerminal("binexp", typeof (LuaBinaryExpressionNode)) {Rule = Expr + BinOp + Expr};
            var UniExp = new NonTerminal("uniexp", typeof (LuaUnaryExpressionNode)) {Rule = UnOp + Expr};
            var ElseIfBlock = new NonTerminal("ElseIfClause", typeof (LuaElseIfNode));
            var ElseIfBlockList = new NonTerminal("ElseIfClause*", typeof(LuaElseIfListNode));
            var ElseBlockOpt = new NonTerminal("ElseClause", typeof(LuaBlockNode));

            var VariableAssignment = new NonTerminal("variable assignment", typeof (LuaAssignmentNode));

            var DoBlock = new NonTerminal("do block", typeof(LuaDoBlockNode));
            var WhileBlock = new NonTerminal("while block", typeof(LuaWhileBlockNode));
            var RepeatBlock = new NonTerminal("repeat block", typeof(LuaRepeatBlockNode));
            var ConditionalBlock = new NonTerminal("conditonal block", typeof (LuaIfNode));
            var ForBlock = new NonTerminal("for block", typeof(LuaForBlockNode));
            var GenericForBlock = new NonTerminal("generic for block", typeof(LuaForBlockNode));

            var LocalFunctionDeclaration = new NonTerminal("local function declaration", typeof(LuaFunctionDefNode));
            var FunctionDeclaration = new NonTerminal("function declaration", typeof (LuaFunctionDefNode));

            var Expr23 = new NonTerminal("expr 23", typeof(LuaExpressionNodeList));

            var SingleStatementWithTermStar = new NonTerminal(SingleStatementWithTermOpt.Name + "*", typeof(LuaNode));
            #endregion

            #region Place Rules Here

            //Using Lua 5.1 grammar as defined in
            //http://www.lua.org/manual/5.1/manual.html#8
            Root = Chunk;

            //chunk ::= {stat [`;´]} [laststat [`;´]]

            SingleStatementWithTermStar.Rule = MakeStarRule(SingleStatementWithTermStar, SingleStatementWithTermOpt);
            Statements.Rule = SingleStatementWithTermStar;
            //Statements.Rule = 
            StatementsEnd.Rule = Empty | LastStatementWithTermOpt;
            Block.Rule = Statements + StatementsEnd;

            //block ::= chunk
            Chunk.Rule = Block;

            //stat ::=  varlist `=´ explist | 
            VariableAssignment.Rule = VarList + EQ + ExprList;

            //     functioncall | 
            //     do block end | 
            //     while exp do block end | 
            //     repeat block until exp | 
            //     if exp then block {elseif exp then block} [else block] end | 
            //     for Name `=´ exp `,´ exp [`,´ exp] do block end | 
            //     for namelist in explist do block end | 
            //     function funcname funcbody | 
            FunctionDeclaration.Rule = FUNCTION + FuncName + FunctionParameters + Block + END;

            //     local function Name funcbody | 
            LocalFunctionDeclaration.Rule = LOCAL + FunctionDeclaration;

            //     local namelist [`=´ explist]
            LocalVariableDeclaration.Rule = LOCAL + NameList;
            LocalVariableDeclarationWithAssignment.Rule = LocalVariableDeclaration + EQ + ExprList;


            DoBlock.Rule = DO + Block + END;
            WhileBlock.Rule = WHILE + Expr + DO + Block + END;
            RepeatBlock.Rule = REPEAT + Block + UNTIL + Expr;

            ElseBlockOpt.Rule = Empty | ELSE + Block;
            ElseIfBlock.Rule = ELSEIF + Expr + THEN + Block;
            ElseIfBlockList.Rule = MakeStarRule(ElseIfBlockList, null, ElseIfBlock);
            ConditionalBlock.Rule = IF + Expr + THEN + Block + ElseIfBlockList + ElseBlockOpt + END;
            ForBlock.Rule = FOR + Name + EQ + Expr23 + DO + Block + END;
            Expr23.Rule = Expr + "," + Expr | Expr + "," + Expr + "," + Expr;
            GenericForBlock.Rule = FOR + NameList + IN + ExprList + DO + Block + END;

            Statement.Rule = VariableAssignment |
                             FunctionCall |
                             DoBlock |
                             WhileBlock |
                             RepeatBlock |
                             ConditionalBlock |
                             ForBlock |
                             GenericForBlock |
                             FunctionDeclaration |
                             LocalFunctionDeclaration |
                             LocalVariableDeclaration |
                             LocalVariableDeclarationWithAssignment;

            //laststat ::= return [explist] | break
            LastStatement.Rule = RETURN + ExprList | RETURN | BREAK;

            //funcname ::= Name {`.´ Name} [`:´ Name]
            //FuncName.Rule = Name + (DOT + Name).Star() + (COLON + Name).Q();
            ColonCallOpt.Rule = Empty | COLON + Name;
            IdentifierWithOptNamespace.Rule = MakePlusRule(IdentifierWithOptNamespace, DOT, Name);
            FuncName.Rule = IdentifierWithOptNamespace + ColonCallOpt;

            //varlist ::= var {`,´ var}
            VarList.Rule = MakePlusRule(VarList, ToTerm(","), Var);

            //namelist ::= Name {`,´ Name}
            NameList.Rule = MakePlusRule(NameList, ToTerm(","), Name);

            //explist ::= {exp `,´} exp
            ExprList.Rule = MakePlusRule(ExprList, ToTerm(","), Expr);
            ExprListOpt.Rule = MakeStarRule(ExprListOpt, ToTerm(","), Expr);

            //exp ::=  nil | false | true | Number | String | `...´ | function | 
            //     prefixexp | tableconstructor | exp binop exp | unop exp 
            Expr.Rule = NIL | FALSE | TRUE | NUMBER | STRING | LONGSTRING | ELLIPSIS | Function |
                        PrefixExpr | TableConstructor | BinExp | UniExp;

            //var ::=  Name | prefixexp `[´ exp `]´ | prefixexp `.´ Name 
            Var.Rule = Name | new NonTerminal("table access", PrefixExpr + "[" + Expr + "]" | PrefixExpr + DOT + Name);

            //prefixexp ::= var | functioncall | `(´ exp `)´
            ParentheticalExpression.Rule = "(" + Expr + ")";
            PrefixExpr.Rule = Var | FunctionCall | ParentheticalExpression;

            //functioncall ::=  prefixexp args | prefixexp `:´ Name args 
            FunctionCall.Rule = PrefixExpr + Args | PrefixExpr + COLON + Name + Args;

            //args ::=  `(´ [explist] `)´ | tableconstructor | String 

            ArgsParameters.Rule = "(" + ExprListOpt + ")" ;
            Args.Rule = ArgsParameters | TableConstructor | STRING | LONGSTRING;

            //function ::= function funcbody
            Function.Rule = FUNCTION + FunctionParameters + Block + END;

            //funcbody ::= `(´ [parlist] `)´ block end
            FunctionParameters.Rule = "(" + ParList + ")";
            //FuncBody.Rule = Block + END;

            //parlist ::= namelist [`,´ `...´] | `...´
            EllipsisOpt.Rule = Empty | ToTerm(",") + ELLIPSIS;
            ParList.Rule = NameList + EllipsisOpt | ELLIPSIS | Empty;

            //tableconstructor ::= `{´ [fieldlist] `}´
  //          TableConstructor.Rule = "{" + FieldList.Q() + "}";
            TableConstructor.Rule = "{" + FieldList + "}";// | "{" + "}";

            //fieldlist ::= field {fieldsep field} [fieldsep]
//            FieldList.Rule = Field + (FieldSep + Field).Star() + FieldSep.Q();
            FieldList.Rule = MakeStarRule(FieldList, FieldSep, Field);

            //field ::= `[´ exp `]´ `=´ exp | Name `=´ exp | exp
            Field.Rule = "[" + Expr + "]" + "=" + Expr | Name + "=" + Expr | Expr;

            //fieldsep ::= `,´ | `;´
            FieldSep.Rule = ToTerm(",") | ";";

            //binop ::= `+´ | `-´ | `*´ | `/´ | `^´ | `%´ | `..´ | 
            //     `<´ | `<=´ | `>´ | `>=´ | `==´ | `~=´ | 
            //     and | or
            BinOp.Rule = ToTerm("+") | MINUS | "*" | "/" | "^" | "%" | CONCAT |
                         "<" | "<=" | ">" | ">=" | "==" | "~=" |
                         AND | OR;

            //unop ::= `-´ | not | `#´
            UnOp.Rule = UMINUS | NOT | GETN;

            #endregion

            #region Define Keywords and Register Symbols

            //RegisterBracePair("(", ")");
            //RegisterBracePair("{", "}");
            //RegisterBracePair("[", "]");

          //  MarkPunctuation(COLON, DOT);
            MarkPunctuation(",", ";");
            MarkPunctuation("(", ")");
            MarkPunctuation("{", "}");
            MarkPunctuation("[", "]");

            RegisterOperators(1, OR);
            RegisterOperators(2, AND);
            RegisterOperators(3, "<", ">", "<=", ">=", "~=", "==");
            RegisterOperators(4, Associativity.Right, CONCAT);
            RegisterOperators(5, MINUS, PLUS);
            RegisterOperators(6, "*", "/", "%");
            RegisterOperators(7, NOT, UMINUS);
            RegisterOperators(8, Associativity.Right, "^");

            #endregion

            LanguageFlags = LanguageFlags.CreateAst |
                            LanguageFlags.SupportsCommandLine | LanguageFlags.TailRecursive;
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
            KeyTerm term = ToTerm(keyword);
            term.SetFlag(TermFlags.IsKeyword, true);
            term.SetFlag(TermFlags.IsReservedWord, true);
            term.EditorInfo = new TokenEditorInfo(TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            term.AstConfig.NodeType = typeof(LuaLiteralNode);
            return term;
        }

        public KeyTerm LiteralKeyword(string keyword)
        {
            KeyTerm term = ToTerm(keyword);
            term.SetFlag(TermFlags.IsKeyword, true);
            term.SetFlag(TermFlags.IsReservedWord, true);
            term.SetFlag(TermFlags.NoAstNode, false);
            term.EditorInfo = new TokenEditorInfo(TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            term.AstConfig.NodeType = typeof(LuaLiteralNode);
            return term;
        }

        public KeyTerm Operator(string op)
        {
            string opCased = CaseSensitive ? op : op.ToLower();
            var term = new KeyTerm(opCased, op);
            term.SetFlag(TermFlags.IsOperator, true);
            term.EditorInfo = new TokenEditorInfo(TokenType.Operator, TokenColor.Text, TokenTriggers.None);
            term.AstConfig.NodeType = typeof(LuaBlockNode);
            return term;
        }

        protected static NumberLiteral CreateLuaNumber(string name)
        {
            var term = new NumberLiteral(name, NumberOptions.AllowStartEndDot);
            //default int types are Integer (32bit) -> LongInteger (BigInt); Try Int64 before BigInt: Better performance?
            term.DefaultIntTypes = new[] {TypeCode.Int32, TypeCode.Int64, NumberLiteral.TypeCodeBigInt};
            term.DefaultFloatType = TypeCode.Double; // it is default
            term.AddPrefix("0x", NumberOptions.Hex);
            term.AstConfig.NodeType = typeof(LuaLiteralNode);
            return term;
        }

        protected static StringLiteral CreateLuaString(string name)
        {
            var term = new LuaStringLiteral(name);
            term.AstConfig.NodeType = typeof(LuaLiteralNode);
            return term;
        }
    }
}