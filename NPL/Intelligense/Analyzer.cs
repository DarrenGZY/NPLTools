using Irony.Interpreter.Ast;
using Irony.Parsing;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using NPLTools.IronyParser;
using NPLTools.IronyParser.Ast;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLTools.Intelligense
{
    public class Analyzer
    {
        private ITextView _textView;
        private IVsTextView _vsTextView;
        private Parser _parser;
        private LuaModel _model;
        private LuaNode _astRoot;
        private ParseTree _parseTree;

        public Analyzer(ITextView textView, IVsTextView vsTextView)
        {
            _textView = textView;
            _vsTextView = vsTextView;

            _textView.TextBuffer.Changed += OnTextBufferChanged;

            _parser = new Parser(LuaGrammar.Instance);
            _parseTree = _parser.Parse(_textView.TextSnapshot.GetText());
            if (_parseTree.Root != null)
                _astRoot = _parseTree.Root.AstNode as LuaNode;
            if (_astRoot != null)
                _model = new LuaModel(_textView, _vsTextView, _astRoot);
        }

        private void OnTextBufferChanged(object sender, TextContentChangedEventArgs e)
        {
            string code = _textView.TextSnapshot.GetText();
            string code2 = e.After.GetText();
            _parseTree = _parser.Parse(_textView.TextSnapshot.GetText());
            if (_parseTree.Root != null)
                _astRoot = _parseTree.Root.AstNode as LuaNode;
            else
                return;

            if (_astRoot != null && _model == null)
                _model = new LuaModel(_textView, _vsTextView, _astRoot);
            if (_astRoot != null && _model != null)
                _model.Update(_astRoot);
        }

        /// <summary>
        /// Go to the definition command
        /// </summary>
        public void GotoDefinition()
        {
            int line, column;
            string text;
            TextSpan[] span = new TextSpan[1];

            _vsTextView.GetCaretPos(out line, out column);
            _vsTextView.GetSelectedText(out text);

            _vsTextView.GetWordExtent(line, column, (uint)WORDEXTFLAGS.WORDEXT_CURRENT, span);
            _vsTextView.SetSelection(span[0].iStartLine, span[0].iStartIndex, span[0].iEndLine, span[0].iEndIndex);
            string word;
            _vsTextView.GetTextStream(span[0].iStartLine, span[0].iStartIndex, span[0].iEndLine, span[0].iEndIndex, out word);

            if (_model != null)
            {
                TextSpan? res = _model.GetDeclarationLocation(word, span[0]);
                if (res != null)
                    _vsTextView.SetCaretPos(res.Value.iStartLine, res.Value.iStartIndex);
            }
        }

        /// <summary>
        /// Format the block command
        /// </summary>
        /// <returns></returns>
        public bool FormatBlock()
        {
            int startLine = _textView.Selection.Start.Position.GetContainingLine().LineNumber;
            int endLine = _textView.Selection.End.Position.GetContainingLine().LineNumber;

            ITextSnapshot snapshot = _textView.TextSnapshot;

            int[] indentations;
            RetrieveIndentationsFromSyntaxTree(_textView, out indentations);
            
            // Need a new parser and scanner here
            Parser parser = new Parser(LuaGrammar.Instance);
            Scanner scanner = parser.Scanner;

            //rule 1: insert space before and after binary operator if there not any
            //rule 2: insert space after comma, semicolon if there not any
            //rule 3: indentation increase inside block
            //rule 4: multiple spaces replaced by a single space
            using (var edit = _textView.TextBuffer.CreateEdit())
            {
                //IEnumerable<ITextSnapshotLine> lines = view.TextSnapshot.Lines;
                for (int lineNumber = startLine; lineNumber <= endLine; lineNumber++)
                {
                    ITextSnapshotLine line = snapshot.GetLineFromLineNumber(lineNumber);
                    int lineOffset = line.Start.Position;
                    string lineText = line.GetText();

                    scanner.VsSetSource(lineText, 0);

                    int state = 0;
                    Token currentToken = scanner.VsReadToken(ref state);
                    Token lastToken = null;
                    // add space before the first token
                    if (currentToken != null)
                    {
                        Span editSpan = new Span(lineOffset, currentToken.Location.Position);
                        string indentation = "";
                        for (int i = 0; i < indentations[lineNumber]; ++i)
                            indentation += "\t";
                        edit.Replace(editSpan, indentation);
                    }

                    while (currentToken != null)
                    {
                        Token nextToken = scanner.VsReadToken(ref state);
                        if (currentToken.Text == "+" ||
                            currentToken.Text == "=" ||
                            currentToken.Text == "*" ||
                            currentToken.Text == "\\" ||
                            currentToken.Text == "-")
                        {
                            if (lastToken != null)
                            {
                                int spaceStart = lastToken.Location.Position + lastToken.Length;
                                int spaceLength = currentToken.Location.Position - spaceStart;
                                if (spaceLength != 1)
                                {
                                    Span span = new Span(lineOffset + spaceStart, spaceLength);
                                    edit.Replace(span, " ");
                                }
                            }

                            if (nextToken != null)
                            {
                                int spaceStart = currentToken.Location.Position + currentToken.Length;
                                int spaceLength = nextToken.Location.Position - spaceStart;
                                if (spaceLength != 1)
                                {
                                    Span span = new Span(lineOffset + spaceStart, spaceLength);
                                    edit.Replace(span, " ");
                                }
                            }
                        }
                        else if (currentToken.Text == "," ||
                            currentToken.Text == ";")
                        {
                            if (nextToken != null)
                            {
                                int spaceStart = currentToken.Location.Position + currentToken.Length;
                                int spaceLength = nextToken.Location.Position - spaceStart;
                                if (spaceLength != 1)
                                {
                                    Span span = new Span(lineOffset + spaceStart, spaceLength);
                                    edit.Replace(span, " ");
                                }
                            }
                        }

                        lastToken = currentToken;
                        currentToken = nextToken;
                    }
                }
                edit.Apply();
            }

            //PrintAstTree(NPLTextViewCreationListener.ParseTree);
            return false;
        }

        public bool CommentOrUncommentBlock(bool comment)
        {
            SnapshotPoint start, end;
            SnapshotPoint? mappedStart, mappedEnd;

            if (_textView.Selection.IsActive && !_textView.Selection.IsEmpty)
            {
                // comment every line in the selection
                start = _textView.Selection.Start.Position;
                end = _textView.Selection.End.Position;
                mappedStart = MapPoint(_textView, start);

                var endLine = end.GetContainingLine();
                if (endLine.Start == end)
                {
                    // http://pytools.codeplex.com/workitem/814
                    // User selected one extra line, but no text on that line.  So let's
                    // back it up to the previous line.  It's impossible that we're on the
                    // 1st line here because we have a selection, and we end at the start of
                    // a line.  In normal selection this is only possible if we wrapped onto the
                    // 2nd line, and it's impossible to have a box selection with a single line.
                    end = end.Snapshot.GetLineFromLineNumber(endLine.LineNumber - 1).End;
                }

                mappedEnd = MapPoint(_textView, end);
            }
            else
            {
                // comment the current line
                start = end = _textView.Caret.Position.BufferPosition;
                mappedStart = mappedEnd = MapPoint(_textView, start);
            }

            if (mappedStart != null && mappedEnd != null &&
                mappedStart.Value <= mappedEnd.Value)
            {
                if (comment)
                {
                    CommentRegion(_textView, mappedStart.Value, mappedEnd.Value);
                }
                else
                {
                    UncommentRegion(_textView, mappedStart.Value, mappedEnd.Value);
                }

                // TODO: select multiple spans?
                // Select the full region we just commented, do not select if in projection buffer 
                // (the selection might span non-language buffer regions)
                if (IsNPLContent(_textView.TextBuffer))
                {
                    UpdateSelection(_textView, start, end);
                }
                return true;
            }

            return false;
        }

        #region format helpers
        private void RetrieveIndentationsFromSyntaxTree(ITextView view, out int[] indentations)
        {
            int lineNumber = view.TextSnapshot.LineCount;
            indentations = new int[lineNumber];
            LuaNode root = _astRoot;
            for (int i = 0; i < lineNumber; ++i)
            {
                indentations[i] = -1;
            }
            IterateAstTree(view, root, indentations);
        }

        private void IterateAstTree(ITextView view, LuaNode node, int[] indentations)
        {
            if (node is LuaBlockNode)
            {
                for (int i = node.Span.Location.Line;
                    i <= view.TextSnapshot.GetLineNumberFromPosition(node.Span.EndPosition - 1); ++i)
                {
                    indentations[i] += 1;
                }
            }
            foreach (LuaNode childNode in node.GetChildNodes())
            {
                IterateAstTree(view, childNode, indentations);
            }
        }

        private static void PrintAstTree(ParseTree tree)
        {
            AstNode root = tree.Root.AstNode as AstNode;
            PrintAstNode(root, 0);
        }

        private static void PrintAstNode(AstNode node, int indent)
        {
            string indents = "";
            for (int i = 0; i < indent; ++i)
                indents += "    ";
            System.Diagnostics.Debug.WriteLine(indents + node + "(" + node.Span.Location.Position + " - " + (node.Span.Location.Position + node.Span.Length) + ")");
            foreach (AstNode child in node.ChildNodes)
            {
                PrintAstNode(child, indent + 1);
            }
        }
        #endregion

        #region comment block helpers
        private bool IsNPLContent(ITextBuffer buffer)
        {
            return buffer.ContentType.IsOfType("NPL");
        }

        private bool IsNPLContent(ITextSnapshot buffer)
        {
            return buffer.ContentType.IsOfType("NPL");
        }

        private SnapshotPoint? MapPoint(ITextView view, SnapshotPoint point)
        {
            return view.BufferGraph.MapDownToFirstMatch(
               point,
               PointTrackingMode.Positive,
               IsNPLContent,
               PositionAffinity.Successor
            );
        }

        /// <summary>
        /// Adds comment characters (--) to the start of each line.  If there is a selection the comment is applied
        /// to each selected line.  Otherwise the comment is applied to the current line.
        /// </summary>
        /// <param name="view"></param>
        private void CommentRegion(ITextView view, SnapshotPoint start, SnapshotPoint end)
        {
            Debug.Assert(start.Snapshot == end.Snapshot);
            var snapshot = start.Snapshot;

            using (var edit = snapshot.TextBuffer.CreateEdit())
            {
                int minColumn = Int32.MaxValue;
                // first pass, determine the position to place the comment
                for (int i = start.GetContainingLine().LineNumber; i <= end.GetContainingLine().LineNumber; i++)
                {
                    var curLine = snapshot.GetLineFromLineNumber(i);
                    var text = curLine.GetText();

                    int firstNonWhitespace = IndexOfNonWhitespaceCharacter(text);
                    if (firstNonWhitespace >= 0 && firstNonWhitespace < minColumn)
                    {
                        // ignore blank lines
                        minColumn = firstNonWhitespace;
                    }
                }

                // second pass, place the comment
                for (int i = start.GetContainingLine().LineNumber; i <= end.GetContainingLine().LineNumber; i++)
                {
                    var curLine = snapshot.GetLineFromLineNumber(i);
                    if (String.IsNullOrWhiteSpace(curLine.GetText()))
                    {
                        continue;
                    }

                    Debug.Assert(curLine.Length >= minColumn);

                    edit.Insert(curLine.Start.Position + minColumn, "--");
                }

                edit.Apply();
            }
        }

        private int IndexOfNonWhitespaceCharacter(string text)
        {
            for (int j = 0; j < text.Length; j++)
            {
                if (!Char.IsWhiteSpace(text[j]))
                {
                    return j;
                }
            }
            return -1;
        }

        /// <summary>
        /// Removes a comment character (--) from the start of each line.  If there is a selection the character is
        /// removed from each selected line.  Otherwise the character is removed from the current line.  Uncommented
        /// lines are ignored.
        /// </summary>
        private void UncommentRegion(ITextView view, SnapshotPoint start, SnapshotPoint end)
        {
            Debug.Assert(start.Snapshot == end.Snapshot);
            var snapshot = start.Snapshot;

            using (var edit = snapshot.TextBuffer.CreateEdit())
            {

                // first pass, determine the position to place the comment
                for (int i = start.GetContainingLine().LineNumber; i <= end.GetContainingLine().LineNumber; i++)
                {
                    var curLine = snapshot.GetLineFromLineNumber(i);

                    DeleteFirstCommentChar(edit, curLine);
                }

                edit.Apply();
            }
        }

        private void UpdateSelection(ITextView view, SnapshotPoint start, SnapshotPoint end)
        {
            view.Selection.Select(
                new SnapshotSpan(
                    // translate to the new snapshot version:
                    start.GetContainingLine().Start.TranslateTo(view.TextBuffer.CurrentSnapshot, PointTrackingMode.Negative),
                    end.GetContainingLine().End.TranslateTo(view.TextBuffer.CurrentSnapshot, PointTrackingMode.Positive)
                ),
                false
            );
        }

        private void DeleteFirstCommentChar(ITextEdit edit, ITextSnapshotLine curLine)
        {
            var text = curLine.GetText();
            for (int j = 0; j < text.Length; j++)
            {
                if (!Char.IsWhiteSpace(text[j]))
                {
                    if (text[j] == '-' && (j + 1) < text.Length && text[j + 1] == '-')
                    {
                        edit.Delete(curLine.Start.Position + j, 2);
                    }
                    break;
                }
            }
        }
        #endregion
    }
}
