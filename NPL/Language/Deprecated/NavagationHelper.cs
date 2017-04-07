using Irony.Parsing;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using NPLTools.Intelligense;
using NPLTools.IronyParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLTools.Language.Editor
{
    internal static class NavagationHelper
    {
        private static LuaModel _model;

        public static void Initialize(LuaModel model)
        {
            _model = model;
        }

        public static void GotoDefinition(IVsTextView vsTextView)
        {
            int line, column;
            string text;
            TextSpan[] span = new TextSpan[1];

            Parser parser = new Parser(LuaGrammar.Instance);
            Scanner scanner = parser.Scanner;
            vsTextView.GetCaretPos(out line, out column);
            vsTextView.GetSelectedText(out text);

            vsTextView.GetWordExtent(line, column, (uint)WORDEXTFLAGS.WORDEXT_CURRENT, span);
            vsTextView.SetSelection(span[0].iStartLine, span[0].iStartIndex, span[0].iEndLine, span[0].iEndIndex);
            string word;
            vsTextView.GetTextStream(span[0].iStartLine, span[0].iStartIndex, span[0].iEndLine, span[0].iEndIndex, out word);

            TextSpan? res = _model.GetDeclarationLocation(word, span[0]);
            if (res != null)
                vsTextView.SetCaretPos(res.Value.iStartLine, res.Value.iStartIndex);
        }

        private static string GetSelectTextFromCaretPosition(int line, int column)
        {
            return "";
        }
    }
}
