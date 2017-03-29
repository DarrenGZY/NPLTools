using Irony.Interpreter.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLTools.IronyParser.Ast
{
    public class BaseNode : AstNode
    {
        public void AddChild(string role, AstNode child)
        {
            if (child == null) return;
            child.Role = role;
            child.Parent = this;
            ChildNodes.Add(child);
        }
    }
}
