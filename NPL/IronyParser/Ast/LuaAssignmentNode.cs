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
                Declaration declaration;
                DeclarationType type = GetDeclarationType(variable, block, out declaration);

            }
        }

        private DeclarationType GetDeclarationType(LuaNode variable, LuaBlockNode block, out Declaration declaration)
        {
            declaration = null;
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
                    List<string> names = new List<string>(variable.AsString.Split('.'));
                    if (localDeclaration.NamesEqual(names))
                        return DeclarationType.None;
                    if (localDeclaration.NamesEqual(names.GetRange(0, names.Count - 1)))
                    {
                        declaration = localDeclaration;
                        return DeclarationType.Local;
                    }
                }

                foreach (var globalDeclaration in block.Globals)
                {
                    List<string> names = new List<string>(variable.AsString.Split('.'));
                    if (globalDeclaration.NamesEqual(names))
                        return DeclarationType.None;
                    if (globalDeclaration.NamesEqual(names.GetRange(0, names.Count - 1)))
                    {
                        declaration = globalDeclaration;
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


