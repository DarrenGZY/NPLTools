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
    public class LuaLocalDeclaration : AstNode
    {
        public AstNodeList VariableList { get; set; }

        public LuaLocalDeclaration ()
        {
            VariableList = new AstNodeList();
        }

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            foreach (ParseTreeNode variable in treeNode.ChildNodes[1].ChildNodes)
            {
                VariableList.Add(AddChild(String.Empty, variable));
            }
        }

    }
}
