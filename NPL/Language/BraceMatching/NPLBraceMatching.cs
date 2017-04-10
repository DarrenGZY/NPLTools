using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace NPLTools.Language.BraceMatching
{
    [Export(typeof(IViewTaggerProvider))]
    [ContentType("NPL")]
    [TagType(typeof(TextMarkerTag))]
    internal class NPLBraceMatchingTaggerProvider : IViewTaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            if (textView == null)
                return null;

            if (textView.TextBuffer != buffer)
                return null;

            return new NPLBraceMatchingTagger(textView, buffer) as ITagger<T>;
        }
    }

    internal class NPLBraceMatchingTagger : ITagger<TextMarkerTag>
    {
        ITextView View { get; set; }
        ITextBuffer SourceBuffer { get; set; }
        SnapshotPoint? CurrentChar { get; set; }
        private Dictionary<char, char> _braceList;

        internal NPLBraceMatchingTagger(ITextView view, ITextBuffer sourceBuffer)
        {
            _braceList = new Dictionary<char, char>();
            _braceList.Add('{', '}');
            _braceList.Add('(', ')');
            _braceList.Add('[', ']');
            this.View = view;
            this.SourceBuffer = sourceBuffer;
            this.CurrentChar = null;

            this.View.Caret.PositionChanged += CaretPositionChanged;
            this.View.LayoutChanged += ViewLayoutChanged;
        }

        private void ViewLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            if (e.NewSnapshot != e.OldSnapshot)
            {
                UpdateAtCaretPosition(View.Caret.Position);
            }
        }

        private void CaretPositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            UpdateAtCaretPosition(e.NewPosition);
        }

        void UpdateAtCaretPosition(CaretPosition caretPosition)
        {
            CurrentChar = caretPosition.Point.GetPoint(SourceBuffer, caretPosition.Affinity);

            if (!CurrentChar.HasValue)
                return;

            var tempEvent = TagsChanged;
            if (tempEvent != null)
                tempEvent(this, new SnapshotSpanEventArgs(new SnapshotSpan(SourceBuffer.CurrentSnapshot, 0, SourceBuffer.CurrentSnapshot.Length)));
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public IEnumerable<ITagSpan<TextMarkerTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0)
                yield break;

            if (!CurrentChar.HasValue || CurrentChar.Value.Position > CurrentChar.Value.Snapshot.Length)
                yield break;

            SnapshotPoint currentChar = CurrentChar.Value;

            if (spans[0].Snapshot != currentChar.Snapshot)
            {
                currentChar = currentChar.TranslateTo(spans[0].Snapshot, PointTrackingMode.Positive);
            }

            char currentText = currentChar.Position == currentChar.Snapshot.Length ? '\0' : currentChar.GetChar();
            SnapshotPoint lastChar = currentChar == 0 ? currentChar : currentChar - 1;
            char lastText = lastChar.GetChar();
            SnapshotSpan pairSpan = new SnapshotSpan();

            if (_braceList.ContainsKey(currentText))
            {
                char closeChar;
                _braceList.TryGetValue(currentText, out closeChar);
                if (FindMatchingCloseChar(currentChar, currentText, closeChar, View.TextViewLines.Count, out pairSpan))
                {
                    yield return new TagSpan<TextMarkerTag>(new SnapshotSpan(currentChar, 1), new TextMarkerTag("blue"));
                    yield return new TagSpan<TextMarkerTag>(pairSpan, new TextMarkerTag("blue"));
                }
            }
            else if(_braceList.ContainsValue(lastText))
            {
                var open = from n in _braceList
                           where n.Value.Equals(lastText)
                           select n.Key;
                if (FindMatchingOpenChar(lastChar, (char)open.ElementAt<char>(0), lastText, View.TextViewLines.Count, out pairSpan))
                {
                    yield return new TagSpan<TextMarkerTag>(new SnapshotSpan(lastChar, 1), new TextMarkerTag("blue"));
                    yield return new TagSpan<TextMarkerTag>(pairSpan, new TextMarkerTag("blue"));
                }
            }

        }

        private bool FindMatchingCloseChar(SnapshotPoint startPoint, char open, char close, int maxLines, out SnapshotSpan pairSpan)
        {
            pairSpan = new SnapshotSpan(startPoint.Snapshot, 1, 1);
            ITextSnapshotLine line = startPoint.GetContainingLine();
            string lineText = line.GetText();
            int lineNumber = line.LineNumber;
            int offset = startPoint.Position - line.Start.Position + 1;

            int stopLineNumber = startPoint.Snapshot.LineCount - 1;
            if (maxLines > 0)
                stopLineNumber = Math.Min(stopLineNumber, lineNumber + maxLines);

            int openCount = 0;
            while (true)
            {
                while (offset < line.Length)
                {
                    char currentChar = lineText[offset];
                    if (currentChar == close)
                    {
                        if (openCount > 0)
                        {
                            openCount--;
                        }
                        else
                        {
                            pairSpan = new SnapshotSpan(startPoint.Snapshot, line.Start + offset, 1);
                            return true;
                        }
                    }
                    else if (currentChar == open)
                    {
                        openCount++;
                    }
                    offset++;
                }
                if (++lineNumber > stopLineNumber)
                    break;
                line = line.Snapshot.GetLineFromLineNumber(lineNumber);
                lineText = line.GetText();
                offset = 0;
            }

            return false;
        }

        private bool FindMatchingOpenChar(SnapshotPoint startPoint, char open, char close, int maxLines, out SnapshotSpan pairSpan)
        {
            pairSpan = new SnapshotSpan(startPoint, startPoint);

            ITextSnapshotLine line = startPoint.GetContainingLine();

            int lineNumber = line.LineNumber;
            int offset = startPoint - line.Start - 1;

            if (offset < 0)
            {
                if (--lineNumber < 0)
                    return false;
                line = line.Snapshot.GetLineFromLineNumber(lineNumber);
                offset = line.Length - 1;
            }

            string lineText = line.GetText();

            int stopLineNumber = 0;
            if (maxLines > 0)
                stopLineNumber = Math.Max(stopLineNumber, lineNumber - maxLines);

            int closeCount = 0;

            while (true)
            {
                while (offset >= 0)
                {
                    char currentChar = lineText[offset];

                    if (currentChar == open)
                    {
                       　if (closeCount > 0)
                        {
                            closeCount--;
                        }
                        else
                        {
                            pairSpan = new SnapshotSpan(line.Start + offset, 1);
                            return true;
                        }
                    }
                    else if (currentChar == close)
                    {
                        closeCount++;
                    }
                    offset--;
                }

                if (--lineNumber < stopLineNumber)
                    break;
                line = line.Snapshot.GetLineFromLineNumber(lineNumber);
                lineText = line.GetText();
                offset = line.Length - 1;
            }
            return false;
        }
    }
}
