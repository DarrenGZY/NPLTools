using Irony.Interpreter.Ast;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLTools.Language
{
    internal static class NPLTextViewExtensions
    {
        public static AstNode AstTree(this ITextView view)
        {

            return new AstNode();
        }
    }
}
