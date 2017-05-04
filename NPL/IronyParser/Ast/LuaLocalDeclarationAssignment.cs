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
    public class LuaLocalDeclarationAssignment : LuaNode, IDeclaration
    {
        public LuaNodeList VariableList { get; set; }
        public LuaNodeList ExpressionList { get; set; }

        public LuaLocalDeclarationAssignment()
        {
            VariableList = new LuaNodeList();
            ExpressionList = new LuaNodeList();
        }

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            LuaLocalDeclaration declaration = treeNode.ChildNodes[0].AstNode as LuaLocalDeclaration;
            VariableList = declaration.VariableList;
            
            foreach (LuaNode variable in VariableList)
            {
                AddChild(variable.Role, variable);
            }

            foreach (ParseTreeNode expression in treeNode.ChildNodes[2].ChildNodes)
            {
                ExpressionList.Add(AddChild(String.Empty, expression) as LuaNode);
            }
        }

        public void GetDeclarations(LuaBlockNode block, LuaModel model)
        {
            for (int i = 0; i < VariableList.Count && i < ExpressionList.Count; ++i)
            {
                LuaNode variable = VariableList[i];
                Declaration declaration = new Declaration(variable.AsString, String.Empty, model.FilePath, 
                                        new ScopeSpan(variable.Span.EndPosition,
                                        variable.EndLine,
                                        block.Span.EndPosition,
                                        block.EndLine));
                block.Locals.Add(declaration);
                
                if (i < ExpressionList.Count)
                {
                    if (ExpressionList[i] is LuaTableNode)
                        AddDeclarationsForTableField(block, model, declaration, ExpressionList[i]);
                    else if (ExpressionList[i] is LuaIdentifierNode ||
                        ExpressionList[i] is LuaTableAccessNode)
                    {
                        Declaration sibling = null;
                        if (DeclarationHelper.TryGetExpressionDeclaration(ExpressionList[i], block, model, out sibling))
                            sibling.AddSibling(declaration);
                    }
                }
            }
        }

        private void AddDeclarationsForTableField(LuaBlockNode block, LuaModel model, Declaration namespaces, LuaNode expr)
        {
            if (expr is LuaTableNode)
            {
                foreach (var field in ((LuaTableNode)expr).FieldList)
                {
                    AddDeclarationsForTableField(block, model, namespaces, field);
                }
            }

            if (expr is LuaField && ((LuaField)expr).Name != null)
            {
                LuaNode variable = ((LuaField)expr).Name;
                Declaration declaration = new Declaration(variable.AsString, String.Empty, model.FilePath,
                        new ScopeSpan(variable.Span.EndPosition,
                        variable.EndLine,
                        block.Span.EndPosition,
                        block.EndLine),
                        namespaces);

                block.Locals.Add(declaration);
                AddDeclarationsForTableField(block, model, declaration, ((LuaField)expr).Expression);
            }
        }
    }
}
