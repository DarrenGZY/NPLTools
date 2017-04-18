using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using System.Diagnostics;
using System;
using IServiceProvider = System.IServiceProvider;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using NPLTools.Intelligense;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Text;

namespace NPLTools.Language
{
    internal class NPLEditorCommandFilter : IOleCommandTarget
    {
        private readonly ITextView _textView;
        private readonly IVsTextView _vsTextView;
        private readonly IServiceProvider _serviceProvider;
        private readonly IComponentModel _componentModel;

        public NPLEditorCommandFilter(ITextView textView, IVsTextView vsTextView, IServiceProvider serviceProvider)
        {
            _textView = textView;
            _vsTextView = vsTextView;
            _serviceProvider = serviceProvider;
        }
    
        public IOleCommandTarget Next { get; set; }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (pguidCmdGroup == VsMenus.guidStandardCommandSet2K)
            {
                switch ((VSConstants.VSStd2KCmdID)nCmdID)
                {
                    case VSConstants.VSStd2KCmdID.COMMENT_BLOCK:
                    case VSConstants.VSStd2KCmdID.COMMENTBLOCK:
                        CommentOrUncommentBlock(true);
                        break;
                    case VSConstants.VSStd2KCmdID.UNCOMMENT_BLOCK:
                    case VSConstants.VSStd2KCmdID.UNCOMMENTBLOCK:
                        CommentOrUncommentBlock(false);
                        break;
                    case VSConstants.VSStd2KCmdID.FORMATSELECTION:
                        FormatBlock();
                        break;
                }
            }
            else if (pguidCmdGroup == VsMenus.guidStandardCommandSet97)
            {
                switch ((VSConstants.VSStd97CmdID)nCmdID)
                {
                    case VSConstants.VSStd97CmdID.GotoDefn:
                        GotoDefinition();
                        break;
                }
            }

            return Next.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (pguidCmdGroup == VsMenus.guidStandardCommandSet2K)
            {
                for (int i = 0; i < cCmds; i++)
                {
                    switch ((VSConstants.VSStd2KCmdID)prgCmds[i].cmdID)
                    {
                        case VSConstants.VSStd2KCmdID.COMMENT_BLOCK:
                        case VSConstants.VSStd2KCmdID.COMMENTBLOCK:
                        case VSConstants.VSStd2KCmdID.UNCOMMENT_BLOCK:
                        case VSConstants.VSStd2KCmdID.UNCOMMENTBLOCK:
                        case VSConstants.VSStd2KCmdID.FORMATSELECTION:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                            return VSConstants.S_OK;
                    }
                }
            }
            else if (pguidCmdGroup == VsMenus.guidStandardCommandSet97)
            {
                for (int i = 0; i < cCmds; i++)
                {
                    switch ((VSConstants.VSStd97CmdID)prgCmds[i].cmdID)
                    {
                        case VSConstants.VSStd97CmdID.GotoDefn:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                            return VSConstants.S_OK;
                    }
                }
            }
            return Next.QueryStatus(pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        private void GotoDefinition()
        {
            var caret = _textView.Caret.Position.BufferPosition;
            var analysis = _textView.GetAnalysisAtCaret(_serviceProvider);

            var span = analysis.Analyzer.GetDeclarationLocation(analysis, _textView, caret);

            if (span.HasValue)
            {
                if (span.Value.Key == _textView.TextBuffer.GetFilePath())
                {
                    int line, column;
                    _vsTextView.GetLineAndColumn(span.Value.Value.StartPosition, out line, out column);
                    _vsTextView.SetCaretPos(line, column);
                }
                else
                {
                    NPLPackage.NavigateTo(_serviceProvider, span.Value.Key, Guid.Empty, 1, 1); // TODO: translate position to line and col
                }
            }
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

        private void FormatBlock()
        {
            var analysis = _textView.GetAnalysisAtCaret(_serviceProvider);
            analysis.Analyzer.FormatBlock(analysis, _textView);
        }

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
