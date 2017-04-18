using System;
using Irony.Ast;
using Irony.Parsing;

namespace NPLTools.IronyParser.Ast
{
    public class LuaAssignmentNode : LuaNode, IDeclaration
    {
        public LuaNodeList VariableList { get; set; }
        public LuaNodeList ExpressionList { get; set; }

        public LuaAssignmentNode()
        {
            VariableList = new LuaNodeList();
            ExpressionList = new LuaNodeList();
        }

        // Lua's productions allways take lists on both sides of the '='
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            foreach (var parseTreeNode in treeNode.ChildNodes[0].ChildNodes)
                VariableList.Add(AddChild(String.Empty, parseTreeNode) as LuaNode);

            foreach (var parseTreeNode in treeNode.ChildNodes[2].ChildNodes)
                ExpressionList.Add(AddChild(String.Empty, parseTreeNode) as LuaNode);

            AsString = "(assignment)";
        }

        public void GetDeclarations(LuaBlockNode block)
        {
            foreach (var variable in VariableList)
            {
                // string name = variable.ToString();
                // block.Locals.Add(variable);
            }
        }
    }
}


