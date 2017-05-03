using Irony.Interpreter.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Ast;
using Irony.Parsing;
using NPLTools.Intelligense;

namespace NPLTools.IronyParser.Ast
{
    public class LuaLocalDeclaration : LuaNode, IDeclaration
    {
        public LuaNodeList VariableList { get; set; }
        public List<string> DeclarationVaribles;
        public Dictionary<string, AstNode> Variables { get; private set; }
        public LuaLocalDeclaration ()
        {
            VariableList = new LuaNodeList();
            DeclarationVaribles = new List<string>();
            Variables = new Dictionary<string, AstNode>();
        }

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            foreach (ParseTreeNode variable in treeNode.ChildNodes[1].ChildNodes)
            {
                VariableList.Add(AddChild(String.Empty, variable) as LuaNode);
                DeclarationVaribles.Add(variable.Token.Text);
                Variables.Add(variable.Token.Text, (AstNode)variable.AstNode);
            }
        }

        public void GetDeclarations(LuaBlockNode block, LuaModel model)
        {
            foreach (var variable in VariableList)
            {
                block.Locals.Add(new Declaration(variable.AsString, model.FilePath,
                    new ScopeSpan(variable.Span.EndPosition, variable.EndLine, block.Span.EndPosition, block.EndLine)));
            }
        }
    }
}
