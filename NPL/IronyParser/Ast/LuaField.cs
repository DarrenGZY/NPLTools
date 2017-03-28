using Irony.Ast;
using Irony.Interpreter.Ast;
using Irony.Parsing;

namespace NPLTools.IronyParser.Ast
{
    public class LuaField :AstNode 
    {
        public AstNode Target;
        public AstNode Expression;

        
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            if (treeNode.ChildNodes.Count > 1)
            {
                Target = AddChild("key", treeNode.ChildNodes[0]);

                Expression = AddChild("value", treeNode.ChildNodes[2]);

                AsString = "key/value field";

                //EvaluateRef = EvaluateSimple;
            }
            else
            {
                Target = AddChild("entry", treeNode.ChildNodes[0]);

                AsString = "list field";
            }

        }

        //private void EvaluateSimple(EvaluationContext context, AstMode mode)
        //{
        //    Expression.Evaluate(context, AstMode.Read);
        //    Target.Evaluate(context, AstMode.Write); //writes the value into the slot
        //}
    }
}