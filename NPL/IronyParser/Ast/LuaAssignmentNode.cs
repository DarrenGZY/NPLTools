using System;
using Irony.Ast;
using Irony.Parsing;
using System.Collections.Generic;
using NPLTools.Intelligense;

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
                    isDeclarationAssign = TryGetExpressionDeclaration(ExpressionList[i], block, model, out sibling);

                DeclarationType type = GetDeclarationType(variable, block, out namespaces);

                if (type == DeclarationType.Global && variable is LuaIdentifierNode)
                {
                    Declaration declaration = new Declaration(variable.AsString, model.FilePath, new ScopeSpan(variable.Span.EndPosition, variable.EndLine, int.MaxValue, int.MaxValue));
                    if (isDeclarationAssign) sibling.AddSibling(declaration);
                    block.Globals.Add(declaration);
                }
                else if (type == DeclarationType.Global && variable is LuaTableAccessNode)
                {
                    string[] names = variable.AsString.Split('.');
                    Declaration declaration = new Declaration(names[names.Length - 1], model.FilePath, new ScopeSpan(variable.Span.EndPosition, variable.EndLine,
                        int.MaxValue, int.MaxValue), namespaces);
                    if (isDeclarationAssign) sibling.AddSibling(declaration);
                    block.Globals.Add(declaration);
                }
                else if (type == DeclarationType.Local)
                {
                    string[] names = variable.AsString.Split('.');
                    Declaration declaration = new Declaration(names[names.Length - 1], model.FilePath, new ScopeSpan(variable.Span.EndPosition, variable.EndLine,
                        block.Span.EndPosition, block.EndLine), namespaces);
                    if (isDeclarationAssign) sibling.AddSibling(declaration);
                    block.Locals.Add(declaration);
                }
            }
        }

        private bool TryGetExpressionDeclaration(LuaNode expr, LuaBlockNode block, LuaModel model, out Declaration declaration)
        {
            declaration = null;
            if (expr is LuaIdentifierNode)
            {
                foreach (var localDeclaration in block.Locals)
                {
                    if (expr.AsString == localDeclaration.Name)
                    {
                        declaration = localDeclaration;
                        return true;
                    }
                }
                foreach (var globalDeclaration in block.Globals)
                {
                    if (expr.AsString == globalDeclaration.Name)
                    {
                        declaration = globalDeclaration;
                        return true;
                    }
                }
                foreach (var globalDeclaration in model.GetGlobalDeclarationInProject())
                {
                    if (expr.AsString == globalDeclaration.Name)
                    {
                        declaration = globalDeclaration;
                        // clear siblings in case of adding duplicate declaration
                        declaration.ClearSiblingsinFile(model.FilePath);
                        return true;
                    }
                }
            }
            else if (expr is LuaTableAccessNode)
            {
                foreach (var localDeclaration in block.Locals)
                {
                    Declaration dummyDeclaration = BuildDeclaration(expr.AsString);
                    if (dummyDeclaration.Equal(localDeclaration))
                    {
                        declaration = localDeclaration;
                        return true;
                    }
                }
                foreach (var globalDeclaration in block.Globals)
                {
                    Declaration dummyDeclaration = BuildDeclaration(expr.AsString);
                    if (dummyDeclaration.Equal(globalDeclaration))
                    {
                        declaration = globalDeclaration;
                        return true;
                    }
                }
                foreach (var globalDeclaration in model.GetGlobalDeclarationInProject())
                {
                    Declaration dummyDeclaration = BuildDeclaration(expr.AsString);
                    if (dummyDeclaration.Equal(globalDeclaration))
                    {
                        declaration = globalDeclaration;
                        // clear siblings in case of adding duplicate declaration
                        declaration.ClearSiblingsinFile(model.FilePath);
                        return true;
                    }
                }
            }
            return false;
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
                    Declaration dummyDeclaration = BuildDeclaration(variable.AsString);
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
                    Declaration dummyDeclaration = BuildDeclaration(variable.AsString);
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

        private Declaration BuildDeclaration(string name)
        {
            int index = name.LastIndexOf('.');
            if (index == -1)
                return new Declaration(name, "");
            else
            {
                //string a = name.Substring(index+1);
                //string b = name.Substring(0, index);
                return new Declaration(name.Substring(index + 1), "", BuildDeclaration(name.Substring(0, index)));
            }
        }

        enum DeclarationType
        {
            Global = 0,
            Local = 1,
            None = 2
        }
    }
}


