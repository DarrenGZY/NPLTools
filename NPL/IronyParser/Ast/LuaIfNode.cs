using Irony.Ast;
using Irony.Interpreter;
using Irony.Interpreter.Ast;
using Irony.Parsing;

namespace NPLTools.IronyParser.Ast
{
    public class LuaIfNode : LuaNode
    {
        public LuaNode Test;
        public LuaNode IfTrue;
        public LuaNode IfFalse;

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            Test = AddChild("Test", treeNode.ChildNodes[1]) as LuaNode;

            IfTrue = AddChild("IfTrue", treeNode.ChildNodes[3]) as LuaNode;

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
                    IfFalse = AddChild("IfFalse", variable.ChildNodes[1]) as LuaNode;
            }
        }
    }

    public class LuaElseIfNode : LuaIfNode
    {
        
    }
}