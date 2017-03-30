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
    public class LuaLocalDeclarationAssignment : LuaNode
    {
        public AstNodeList VariableList { get; set; }
        public AstNodeList ExpressionList { get; set; }

        public LuaLocalDeclarationAssignment()
        {
            VariableList = new AstNodeList();
            ExpressionList = new AstNodeList();
        }

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            LuaLocalDeclaration declaration = treeNode.ChildNodes[0].AstNode as LuaLocalDeclaration;
            VariableList = declaration.VariableList;
            
            foreach (AstNode variable in VariableList)
            {
                AddChild(variable.Role, variable);
            }

            foreach (ParseTreeNode expression in treeNode.ChildNodes[2].ChildNodes)
            {
                ExpressionList.Add(AddChild(String.Empty, expression));
            }
        }


    }
}
