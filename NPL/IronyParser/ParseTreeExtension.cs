using Irony.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLTools.IronyParser
{
    internal static class ParseTreeExtension
    {
        internal static int GetEndLine(this ParseTreeNode node)
        {
            if (node.ChildNodes.Count == 0)
                return node.Span.Location.Line;
            else
                return node.ChildNodes[node.ChildNodes.Count - 1].GetEndLine();
        }
    }
}
