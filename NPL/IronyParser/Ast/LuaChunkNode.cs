using Irony.Ast;
using Irony.Parsing;
using System.Collections.Generic;
using System.Text;

namespace NPLTools.IronyParser.Ast
{
    public class LuaChunkNode : LuaNode
    {
        public LuaBlockNode Block { get; set; }
        public List<LuaNode> Globals { get; set; }

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            // Normally Chunk only has one child node, which is a block node
            if (treeNode.ChildNodes.Count > 0)
                Block = AddChild(string.Empty, treeNode.ChildNodes[0]) as LuaBlockNode;

            AsString = "LuaChunk";
        }

        public override void AppendAsString(StringBuilder res, string indentation)
        {
            Block.AppendAsString(res, string.Empty);
        }
    }
}