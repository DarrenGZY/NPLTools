using Irony.Interpreter.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Ast;
using Irony.Parsing;

namespace NPLTools.IronyParser.Ast
{
    public class LuaLiteralNode : AstNode
    {
        public LuaType Type { get; private set; }

        public string Value { get; private set; }

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            Value = treeNode.Token.Text;
            switch (treeNode.Term.Name)
            {
                case "nil":
                    Type = LuaType.Nil;
                    break;
                case "false":
                    Type = LuaType.Boolean;
                    break;
                case "true":
                    Type = LuaType.Boolean;
                    break;
                case "number":
                    Type = LuaType.Number;
                    break;
                case "string":
                    Type = LuaType.String;
                    break;
                default:
                    Type = LuaType.Unknown;
                    break;
            }

            AsString = Value + "(" + Type + ")";
        }
    }

    public enum LuaType
    {
        Unknown,
        Nil,
        Boolean,
        Number,
        String
    }
}
