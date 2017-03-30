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
    public class LuaElseIfListNode : AstNode
    {
        public List<LuaElseIfNode> ElseIfNodes { get; private set; }

        public LuaElseIfListNode()
        {
            ElseIfNodes = new List<LuaElseIfNode>();
        }

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            foreach (ParseTreeNode child in treeNode.ChildNodes)
            {
                ElseIfNodes.Add(AddChild("elseif node", child) as LuaElseIfNode);
            }
        }
    }
}
