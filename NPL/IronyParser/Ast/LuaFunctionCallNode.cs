using System;
using Irony.Ast;
using Irony.Parsing;
using NPLTools.Intellisense;

namespace NPLTools.IronyParser.Ast
{
    public class LuaFunctionCallNode : LuaNode, IDeclaration
    {
        LuaNode Target;
        LuaNode Arguments;
        LuaNode Name;
        string _targetName;

        public LuaFunctionCallNode()
        {

        }

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            string name = "";
            // prefixexp args
            if (treeNode.ChildNodes.Count == 2)
            {
                Target = AddChild(String.Empty, treeNode.ChildNodes[0]) as LuaNode;
                Arguments = AddChild(String.Empty, treeNode.ChildNodes[1]) as LuaNode;
                Name = null;
            }
            // prefixexp `:´ Name args 
            else if (treeNode.ChildNodes.Count == 4)
            {
                Target = AddChild(String.Empty, treeNode.ChildNodes[0]) as LuaNode;
                Name = AddChild(String.Empty, treeNode.ChildNodes[2]) as LuaNode;
                Arguments = AddChild(String.Empty, treeNode.ChildNodes[3]) as LuaNode;
            }
            
            //foreach (var node in treeNode.ChildNodes)
            //{
            //    if (node.Term.Name == "identifier")
            //        name += node.FindTokenAndGetText();

            //    if (node.Term.Flags.IsSet(TermFlags.IsOperator))
            //        name += node.Term.Name;

            //    if (node.Term.Name == "expr list")
            //        Arguments = AddChild("Args", node);
            //}

            _targetName = name;
            
            AsString = "Call " + _targetName;
        }

        public void GetDeclarations(LuaBlockNode block, LuaModel model)
        {
            if (Target.AsString == "require" && Arguments is LuaLiteralNode)
            {
                LuaLiteralNode filePathNode = Arguments as LuaLiteralNode;
                block.Requires.Add(new RequiredDeclaration(filePathNode.Value, new ScopeSpan(this.Span.EndPosition, this.EndLine, int.MaxValue, int.MaxValue)));
            }
        }
    }//class
}
