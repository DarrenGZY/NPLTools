using System.Collections;
using System.Collections.Generic;
using Irony.Ast;
using Irony.Parsing;
using Irony.Interpreter.Ast;

namespace NPLTools.IronyParser.Ast
{
    public class LuaParmListNode : LuaNode
    {
        static void RecursiveChildTraversal(List<ParseTreeNode> leafNodes, ParseTreeNode node) {
            if (node.ChildNodes.Count == 0)
            {
                leafNodes.Add(node);
                return;
            }

            foreach (var childNode in node.ChildNodes)
            {
                RecursiveChildTraversal(leafNodes, childNode);
            }
        }

        public new void Init(AstContext context, ParseTreeNode treeNode) {
            base.Init(context, treeNode);

            var parms = new List<ParseTreeNode>();
            RecursiveChildTraversal(parms, treeNode);

            foreach (var parm in parms)
            {
                AddChild("parameter", parm);

            }
            //foreach (var child in treeNode.ChildNodes)
            //{
            //    if (child.ChildNodes.Count > 0)
            //        foreach (var childNode in child.ChildNodes[0].ChildNodes)
            //        {
            //            AddChild("parameter", childNode);

            //            parms.Add(childNode.Term);
            //        }
            //    else
            //        AddChild("parameter", child);
            //}
            AsString = " (";
            parms.ForEach(obj => AsString += ", " + obj );
            AsString += ")";
        }
    }
}