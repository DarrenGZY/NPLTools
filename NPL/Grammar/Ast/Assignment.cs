using System;
using System.Collections.Generic;
using Irony.Ast;
using Irony.Parsing;

namespace NPLTools.Grammar.Ast
{

    public class Assignment : Node
    {
        public Node VariableList { get; set; }

        public Node ExpressionList { get; set; }

        public bool IsLocal { get; set; }

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            if (treeNode.ChildNodes.Count != 3)
            {
                return;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("in Assignment");
                VariableList = treeNode.ChildNodes[0].AstNode as Node;
                ExpressionList = treeNode.ChildNodes[2].AstNode as Node;
            }
        }
    }
}
