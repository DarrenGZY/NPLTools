using Irony.Interpreter.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Ast;
using Irony.Parsing;

namespace NPLTools.IronyParser.Ast
{
    public class LuaBlockNode : LuaNode
    {
        public AstNodeList Statements
        {
            get { return ChildNodes; }
        }

        public List<LuaNode> Locals;

        public LuaBlockNode()
        {
            Locals = new List<LuaNode>();
        }

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            ParseTreeNode statements = treeNode.ChildNodes.Count > 0 ? treeNode.ChildNodes[0] : null;
            if (statements == null) return;

            foreach (ParseTreeNode child in statements.ChildNodes)
            {
                AddChild(String.Empty, child);

                if (child.AstNode is IDeclaration)
                {
                    ((IDeclaration)child.AstNode).GetDeclarations(this);
                }
            }

            AsString = "Block";
        }
    }
}
