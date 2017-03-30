using Irony.Ast;
using Irony.Interpreter.Ast;
using Irony.Parsing;

namespace NPLTools.IronyParser.Ast
{
    public class LuaAssignmentNode : LuaNode
    {
        public LuaNode Target;
        public string AssignmentOp;
        public LuaNode Expression;

        // Lua's productions allways take lists on both sides of the '='
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            Target = AddChild("To", treeNode.ChildNodes[0]) as LuaNode;
            
            AssignmentOp = "=";

            Expression = AddChild("Expr", treeNode.ChildNodes[2]) as LuaNode;

            AsString = AssignmentOp + " (assignment)";
        }
    }
}


