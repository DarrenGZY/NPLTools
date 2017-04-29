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
        private ParseTreeNode _treeNode;

        public void AddChild(string role, LuaNode child)
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
            _treeNode = treeNode;
        }

        public int EndLine
        {
            get { return _treeNode.GetEndLine(); }
        }
    }

    public class LuaNodeList : List<LuaNode> { }
}
