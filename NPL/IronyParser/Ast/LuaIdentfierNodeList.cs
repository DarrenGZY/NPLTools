using Irony.Ast;
using Irony.Interpreter.Ast;
using Irony.Parsing;
using System.Collections.Generic;

namespace NPLTools.IronyParser.Ast
{
    public class LuaIdentifierNodeList : LuaNode
    {
        public List<LuaIdentifierNode> Identifiers = new List<LuaIdentifierNode>();

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            foreach (var child in treeNode.ChildNodes)
            {
                Identifiers.Add(AddChild("id ", child) as LuaIdentifierNode);
            }
            AsString = "identifier list";
        }
    }
}


