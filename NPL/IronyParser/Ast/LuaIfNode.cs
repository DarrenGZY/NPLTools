using Irony.Ast;
using Irony.Interpreter;
using Irony.Interpreter.Ast;
using Irony.Parsing;

namespace NPLTools.IronyParser.Ast
{
    public class LuaIfNode : AstNode
    {
        public AstNode Test;
        public AstNode IfTrue;
        public AstNode IfFalse;

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            Test = AddChild("Test", treeNode.ChildNodes[1]);

            IfTrue = AddChild("IfTrue", treeNode.ChildNodes[3]);

            foreach (ParseTreeNode variable in treeNode.ChildNodes)
            {
                if (variable.ToString() == "ElseIfClause*" && variable.ChildNodes.Count > 0)
                {
                    foreach (var elseIf in variable.ChildNodes)
                    {
                        AddChild("ElseIf", elseIf);                        
                    }
                }

                if (variable.ToString() == "ElseClause" && variable.ChildNodes.Count > 0)
                    IfFalse = AddChild("IfFalse", variable.ChildNodes[1]);
            }
        }

        //public override void EvaluateNode(EvaluationContext context, AstMode mode)
        //{
        //    Test.Evaluate(context, AstMode.Write);
        //    object result = context.Data.Pop();
        //    if (context.Runtime.IsTrue(result))
        //    {
        //        if (IfTrue != null) IfTrue.Evaluate(context, AstMode.Read);
        //    }
        //    else
        //    {
        //        if (IfFalse != null) IfFalse.Evaluate(context, AstMode.Read);
        //    }
        //}
    }

    public class LuaElseIfNode : LuaIfNode
    {
        
    }
}