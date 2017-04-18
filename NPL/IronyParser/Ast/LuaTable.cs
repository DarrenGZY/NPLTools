using System;
using Irony.Ast;
using Irony.Parsing;
using Irony.Interpreter.Ast;

namespace NPLTools.IronyParser.Ast
{
    public class LuaTableNode : LuaNode
    {
        public LuaNodeList FieldList;

        public LuaTableNode()
        {
            FieldList = new LuaNodeList();
        }

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            foreach (var node in treeNode.ChildNodes)
                FieldList.Add(AddChild(String.Empty, node) as LuaNode);
            AsString = "LuaTable";
        }
    }
}
