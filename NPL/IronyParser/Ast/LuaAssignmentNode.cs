using System;
using Irony.Ast;
using Irony.Parsing;
using System.Collections.Generic;
using NPLTools.Intellisense;

namespace NPLTools.IronyParser.Ast
{
    public class LuaAssignmentNode : LuaNode, IDeclaration
    {
        public LuaNodeList VariableList { get; set; }
        public LuaNodeList ExpressionList { get; set; }

        public LuaAssignmentNode()
        {
            VariableList = new LuaNodeList();
            ExpressionList = new LuaNodeList();
        }

        // Lua's productions allways take lists on both sides of the '='
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            foreach (var parseTreeNode in treeNode.ChildNodes[0].ChildNodes)
                VariableList.Add(AddChild(String.Empty, parseTreeNode) as LuaNode);

            foreach (var parseTreeNode in treeNode.ChildNodes[2].ChildNodes)
                ExpressionList.Add(AddChild(String.Empty, parseTreeNode) as LuaNode);

            AsString = "(assignment)";
        }

        public void GetDeclarations(LuaBlockNode block, LuaModel model)
        {
            for (int i = 0; i < VariableList.Count; ++i)
            {
                LuaNode variable = VariableList[i];
                Declaration namespaces, sibling = null; 
                bool isDeclarationAssign = false;
                if (i < ExpressionList.Count)
                    isDeclarationAssign = DeclarationHelper.TryGetExpressionDeclaration(ExpressionList[i], block, model, out sibling);

                DeclarationType type = GetDeclarationType(variable, block, out namespaces);

                if (type == DeclarationType.Global && variable is LuaIdentifierNode)
                {
                    Declaration declaration = new Declaration(variable.AsString, String.Empty, model.FilePath, new ScopeSpan(variable.Span.EndPosition, variable.EndLine, int.MaxValue, int.MaxValue));
                    if (isDeclarationAssign) sibling.AddSibling(declaration);
                    block.Globals.Add(declaration);
                }
                else if (type == DeclarationType.Global && variable is LuaTableAccessNode)
                {
                    string[] names = variable.AsString.Split('.');
                    Declaration declaration = new Declaration(names[names.Length - 1], String.Empty, model.FilePath, new ScopeSpan(variable.Span.EndPosition, variable.EndLine,
                        int.MaxValue, int.MaxValue), namespaces);
                    if (isDeclarationAssign) sibling.AddSibling(declaration);
                    block.Globals.Add(declaration);
                }
                else if (type == DeclarationType.Local)
                {
                    string[] names = variable.AsString.Split('.');
                    Declaration declaration = new Declaration(names[names.Length - 1], String.Empty, model.FilePath, new ScopeSpan(variable.Span.EndPosition, variable.EndLine,
                        block.Span.EndPosition, block.EndLine), namespaces);
                    if (isDeclarationAssign) sibling.AddSibling(declaration);
                    block.Locals.Add(declaration);
                }
            }
        }

        

        private DeclarationType GetDeclarationType(LuaNode variable, LuaBlockNode block, out Declaration namespaces)
        {
            namespaces = null;
            if (variable is LuaIdentifierNode)
            {
                foreach (var localDeclaration in block.Locals)
                {
                    if (variable.AsString == localDeclaration.Name)
                    {
                        return DeclarationType.None;
                    }
                }

                foreach (var globalDeclaration in block.Globals)
                {
                    if (variable.AsString == globalDeclaration.Name)
                    {
                        return DeclarationType.None;
                    }
                }
            }
            else if (variable is LuaTableAccessNode)
            {
                foreach (var localDeclaration in block.Locals)
                {
                    //List<string> names = new List<string>(variable.AsString.Split('.'));
                    Declaration dummyDeclaration = DeclarationHelper.BuildDeclaration(variable.AsString);
                    if (dummyDeclaration.Equal(localDeclaration))
                        return DeclarationType.None;
                    if (dummyDeclaration.NameSpace != null && dummyDeclaration.NameSpace.Equal(localDeclaration))
                    {
                        namespaces = localDeclaration;
                        return DeclarationType.Local;
                    }
                }

                foreach (var globalDeclaration in block.Globals)
                {
                    //List<string> names = new List<string>(variable.AsString.Split('.'));
                    Declaration dummyDeclaration = DeclarationHelper.BuildDeclaration(variable.AsString);
                    if (dummyDeclaration.Equal(globalDeclaration))
                        return DeclarationType.None;
                    if (dummyDeclaration.NameSpace != null && dummyDeclaration.NameSpace.Equal(globalDeclaration))
                    {
                        namespaces = globalDeclaration;
                        return DeclarationType.Global;
                    }
                }
            }

            return DeclarationType.Global;
        }

        enum DeclarationType
        {
            Global = 0,
            Local = 1,
            None = 2
        }
    }
}


