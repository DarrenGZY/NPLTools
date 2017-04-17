using System;
using Irony.Ast;
using Irony.Interpreter.Ast;
using Irony.Parsing;

namespace NPLTools.IronyParser.Ast
{
    public class LuaFunctionCallNode : LuaNode
    {
        AstNode TargetRef;
        AstNode Arguments;
        string _targetName;

        public LuaFunctionCallNode()
        {

        }

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            string name = "";
            foreach (var node in treeNode.ChildNodes)
            {
                if (node.Term.Name == "identifier")
                    name += node.FindTokenAndGetText();

                if (node.Term.Flags.IsSet(TermFlags.IsOperator))
                    name += node.Term.Name;

                if (node.Term.Name == "expr list")
                    Arguments = AddChild("Args", node);
            }

            _targetName = name;
            
            AsString = "Call " + _targetName;
        }

    }//class
}
