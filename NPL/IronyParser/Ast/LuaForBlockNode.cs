using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Ast;
using Irony.Parsing;

namespace NPLTools.IronyParser.Ast
{
    public class LuaForBlockNode : LuaNode
    {
        public LuaIdentifierNode Identifier { get; private set; }
        // For generic loop only
        public LuaIdentifierNodeList IdentifierList { get; private set; }
        public LuaExpressionNodeList ExpressionList { get; private set; }
        public LuaBlockNode Block { get; private set; }

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            // All for loop parse tree node has seven children node
            if (treeNode.ChildNodes.Count != 7) return;

            // Generic for loop ("for a, b in 1, 2 do end")
            if (treeNode.ChildNodes[1].AstNode is LuaIdentifierNodeList)
            {
                IdentifierList = treeNode.ChildNodes[1].AstNode as LuaIdentifierNodeList;
                AddChild("forloop id list", IdentifierList);
                ExpressionList = treeNode.ChildNodes[3].AstNode as LuaExpressionNodeList;
                AddChild("forloop expr list", ExpressionList);
                Block = treeNode.ChildNodes[5].AstNode as LuaBlockNode;
                AddChild("forloop blck", Block);
            }
            else if (treeNode.ChildNodes[1].AstNode is LuaIdentifierNode)
            {
                Identifier = treeNode.ChildNodes[1].AstNode as LuaIdentifierNode;
                AddChild("forloop id", Identifier);
                ExpressionList = treeNode.ChildNodes[3].AstNode as LuaExpressionNodeList;
                AddChild("forloop expr list", ExpressionList);
                Block = treeNode.ChildNodes[5].AstNode as LuaBlockNode;
                AddChild("forloop blck", Block);
            }

        }
    }
}
