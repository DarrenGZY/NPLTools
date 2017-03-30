using Irony.Interpreter.Ast;
using Irony.Parsing;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
//using NPLTools.Grammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLTools.Language.Editor
{
    internal static class FormatHelper
    {
        public static bool FormatBlock(ITextView view)
        {
            int[] indentations;
            AstNode root = NPLTextViewCreationListener.AstRoot;
            using (var edit = view.Selection.Start.Position.Snapshot.TextBuffer.CreateEdit())
            {
                edit.Insert(0, "hahaha");
                edit.Apply();
            }
                
            retrieveIndentationsFromSyntaxTree(view, out indentations);
            return false;
        }

        private static void retrieveIndentationsFromSyntaxTree(ITextView view, out int[] indentations)
        {
            int lineNumber = view.TextSnapshot.LineCount;
            indentations = new int[lineNumber];
            string text = view.TextSnapshot.GetText();
            
            //Parser parser = new Parser(LuaGrammar.Instance);
            //ParseTree syntaxTree = parser.Parse(text);
        }
    }
}
