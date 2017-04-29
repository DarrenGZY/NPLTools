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

        /// <summary>
        /// LuaTable only has one child LuaFieldList
        /// </summary>
        /// <param name="context"></param>
        /// <param name="treeNode"></param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            foreach (var node in treeNode.ChildNodes[0].ChildNodes)
            {
                LuaFieldAndSeperatorNode fieldAndSep = node.AstNode as LuaFieldAndSeperatorNode;
                AddChild(String.Empty, fieldAndSep.Field);
                FieldList.Add(fieldAndSep.Field);
            }
                
            AsString = "LuaTable";
        }
    }
}
