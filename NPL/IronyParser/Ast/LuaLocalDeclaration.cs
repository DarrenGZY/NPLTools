using Irony.Interpreter.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Ast;
using Irony.Parsing;
using NPLTools.Intellisense;

namespace NPLTools.IronyParser.Ast
{
    public class LuaLocalDeclaration : LuaNode, IDeclaration
    {
        public LuaNodeList VariableList { get; set; }
        public LuaLocalDeclaration ()
        {
            VariableList = new LuaNodeList();
        }

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            foreach (ParseTreeNode variable in treeNode.ChildNodes[1].ChildNodes)
            {
                VariableList.Add(AddChild(String.Empty, variable) as LuaNode);
            }
        }

        public void GetDeclarations(LuaBlockNode block, LuaModel model)
        {
            foreach (var variable in VariableList)
            {
                block.Locals.Add(new Declaration(variable.AsString, String.Empty, model.FilePath,
                    new ScopeSpan(variable.Span.EndPosition, variable.EndLine, block.Span.EndPosition, block.EndLine)));
            }
        }
    }
}
