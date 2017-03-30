using Irony.Ast;
using Irony.Interpreter.Ast;
using Irony.Parsing;

namespace NPLTools.IronyParser.Ast
{
    public class LuaField : LuaNode 
    {
        public LuaNode Target { get; private set; }
        public LuaNode Expression { get; private set; }

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            if (treeNode.ChildNodes.Count > 1)
            {
                Target = AddChild("key", treeNode.ChildNodes[0]) as LuaNode;
                Expression = AddChild("value", treeNode.ChildNodes[2]) as LuaNode;
                AsString = "key/value field";
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