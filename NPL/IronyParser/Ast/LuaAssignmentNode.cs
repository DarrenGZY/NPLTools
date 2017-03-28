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
    }
}


