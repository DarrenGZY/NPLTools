using System;
using Irony.Ast;
using Irony.Parsing;
using System.Collections.Generic;

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

        public void GetDeclarations(LuaBlockNode block)
        {
            for (int i = 0; i < VariableList.Count; ++i)
            {
                LuaNode variable = VariableList[i];
                Declaration namespaces;
                DeclarationType type = GetDeclarationType(variable, block, out namespaces);
                if (type == DeclarationType.Global && variable is LuaIdentifierNode)
                {
                    Declaration declaration = new Declaration(variable.AsString, new ScopeSpan(variable.Span.EndPosition, variable.EndLine, int.MaxValue, int.MaxValue));
                    block.Globals.Add(declaration);

                }
                else if (type == DeclarationType.Global && variable is LuaTableAccessNode)
                {
                    string[] names = variable.AsString.Split('.');
                    block.Globals.Add(new Declaration(names[names.Length - 1], new ScopeSpan(variable.Span.EndPosition, variable.EndLine,
                        int.MaxValue, int.MaxValue), namespaces));
                }
                else if (type == DeclarationType.Local)
                {
                    string[] names = variable.AsString.Split('.');
                    block.Locals.Add(new Declaration(names[names.Length - 1], new ScopeSpan(variable.Span.EndPosition, variable.EndLine,
                        block.Span.EndPosition, block.EndLine), namespaces ));
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
                    Declaration dummyDeclaration = new Declaration(variable.AsString);
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
                    Declaration dummyDeclaration = new Declaration(variable.AsString);
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


