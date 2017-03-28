using Irony.Ast;
using Irony.Interpreter.Ast;
using Irony.Parsing;

namespace NPLTools.IronyParser.Ast
{
    //A node representing expression list - for example, list of argument expressions in function call
    public class LuaIdentifierNodeList : AstNode
    {
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            foreach (var child in treeNode.ChildNodes)
            {
                AddChild("id ", child);
            }
            AsString = "identifier list";
        }

        //public override void EvaluateNode(EvaluationContext context, AstMode mode)
        //{
        //    var result = new ValuesList();
        //    foreach (var expr in ChildNodes)
        //    {
        //        expr.Evaluate(context, AstMode.Read);
        //        result.Add(context.Data.Pop());
        //    }
        //    //Push list on the stack
        //    context.Data.Push(result);
        //}

    }//class
}


