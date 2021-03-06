﻿using Irony.Ast;
using Irony.Parsing;
using Irony.Interpreter.Ast;

namespace NPLTools.IronyParser.Ast {

    public class LuaUnaryExpressionNode : LuaNode {
        public string Op;
        public string UnaryOp; 
        public AstNode Argument;

        public LuaUnaryExpressionNode() { }
        public override void Init(AstContext context, ParseTreeNode treeNode) {
            base.Init(context, treeNode);
            Op = treeNode.ChildNodes[0].FindTokenAndGetText();
            Argument = AddChild("Arg", treeNode.ChildNodes[1]);
            base.AsString = Op + "(unary op)";
            // setup evaluation method;
            //switch (Op) {
            //    case "+": EvaluateRef = EvaluatePlus; break;
            //    case "-": EvaluateRef = EvaluateMinus; break;
            //    case "!": EvaluateRef = EvaluateNot; break;
            //    case "not": EvaluateRef = EvaluateNot; break;

            //    default:
            //        break;
                    //  string msg = string.Format(Resources.ErrNoImplForUnaryOp, Op);
                    // throw new AstException(this, msg);
            //}//switch
        }

        //#region Evaluation methods

        //private void EvaluatePlus(EvaluationContext context, AstMode mode) {
        //    Argument.Evaluate(context, AstMode.Read);
        //}
    
        //private void EvaluateMinus(EvaluationContext context, AstMode mode) {
        //    context.Data.Push((byte)0);
        //    Argument.Evaluate(context, AstMode.Read);
        //    context.CallDispatcher.ExecuteBinaryOperator("-"); 
        //}
    
        //private void EvaluateNot(EvaluationContext context, AstMode mode) {
        //    Argument.Evaluate(context, AstMode.Read);
        //    var value = context.Data.Pop();
        //    var bValue = (bool) context.Runtime.BoolResultConverter(value);
        //    context.Data.Push(!bValue); 
        //}
        //#endregion

    }//class
}

//namespace

