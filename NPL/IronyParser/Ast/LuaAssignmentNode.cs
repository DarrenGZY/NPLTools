#region License

/* **********************************************************************************
 * Copyright (c) Roman Ivantsov
 * This source code is subject to terms and conditions of the MIT License
 * for Irony. A copy of the license can be found in the License.txt file
 * at the root of this distribution. 
 * By using this source code in any fashion, you are agreeing to be bound by the terms of the 
 * MIT License.
 * You must not remove this notice from this software.
 * **********************************************************************************/

#endregion

using Irony.Ast;
using Irony.Interpreter.Ast;
using Irony.Parsing;

namespace NPLTools.IronyParser.Ast
{
    public class LuaAssignmentNode : AstNode
    {
        public AstNode Target;
        public string AssignmentOp;
        public AstNode Expression;

        // Lua's productions allways take lists on both sides of the '='
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            Target = AddChild("To", treeNode.ChildNodes[0]);
            
            AssignmentOp = "=";

            Expression = AddChild("Expr", treeNode.ChildNodes[2]);

            AsString = AssignmentOp + " (assignment)";
            
            //EvaluateRef = EvaluateSimple;
        }

        //private void EvaluateSimple(EvaluationContext context, AstMode mode)
        //{
        //    Expression.Evaluate(context, AstMode.Read);
        //    Target.Evaluate(context, AstMode.Write); //writes the value into the slot
        //}


    }
}


