using Irony.Ast;
using Irony.Interpreter.Ast;
using Irony.Parsing;

namespace NPLTools.IronyParser.Ast
{
    public class LuaChunkNode : AstNode
    {
        public LuaBlockNode Block { get; set; }

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            // Normally Chunk only has one child node, which is a block node
            if (treeNode.ChildNodes.Count > 0)
                Block = AddChild(string.Empty, treeNode.ChildNodes[0]) as LuaBlockNode;

            AsString = "LuaChunk";
        }

    }//class

}

//namespace

