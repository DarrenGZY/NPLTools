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
    public class LuaNode : AstNode
    {
        public void AddChild(string role, AstNode child)
        {
            if (child == null) return;
            child.Role = role;
            child.Parent = this;
            ChildNodes.Add(child);
        }

        public virtual void AppendAsString(StringBuilder res, string indentation)
        {
            
        }

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
        }
    }

    public class LuaNodeList : List<LuaNode> { }
}
