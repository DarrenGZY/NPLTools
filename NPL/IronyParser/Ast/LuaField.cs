using Irony.Ast;
using Irony.Interpreter.Ast;
using Irony.Parsing;

namespace NPLTools.IronyParser.Ast
{
    public class LuaField : LuaNode 
    {
        public LuaNode Name { get; private set; }
        public LuaNode Expression { get; private set; }

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            if (treeNode.ChildNodes.Count > 1)
            {
                Name = AddChild("key", treeNode.ChildNodes[0]) as LuaNode;
                Expression = AddChild("value", treeNode.ChildNodes[2]) as LuaNode;
                AsString = "key/value field";
            }
            else
            {
                Name = null;
                Expression = AddChild("entry", treeNode.ChildNodes[0]) as LuaNode;
                AsString = "list field";
            }
        }
    }
}