using Microsoft.VisualStudio.Text;
using NPLTools.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NPLTools.Language;
using SourceSpan = NPLTools.Intelligense.SourceSpan;
using Microsoft.VisualStudio.Text.Editor;

namespace NPLTools.Intelligense
{
    public class ProjectAnalyzer
    {
        // 
        private Dictionary<string, AnalysisEntry> _projectFiles = new Dictionary<string, AnalysisEntry>();
        private Dictionary<int, AnalysisEntry> _projectFilesById = new Dictionary<int, AnalysisEntry>();
        private Dictionary<int, ITextBuffer> _projectBufferById = new Dictionary<int, ITextBuffer>();
        private readonly IServiceProvider _serviceProvider;
        private readonly AnalysisEntryService _entryService;

        public ProjectAnalyzer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _entryService = _serviceProvider.GetEntryService();
        }

        public bool ContainsFile(string fileName)
        {
            return _projectFiles.ContainsKey(fileName);
        }

        internal AnalysisEntry MonitorTextBuffer(ITextBuffer textBuffer)
        {
            AnalysisEntry entry = CreateProjectEntry(textBuffer);
            _entryService.SetAnalyzer(textBuffer, this);
            textBuffer.Changed += TextBufferChanged;
            return entry;
        }

        internal bool HasMonitoredTextBuffer(ITextBuffer textBuffer)
        {
            string path = textBuffer.GetFilePath();
            return _projectFiles.ContainsKey(path);
        }

        internal void CanceledMonitorTextBuffer(AnalysisEntry entry, ITextBuffer textBuffer)
        {
            string path = entry.FilePath;
            if (_projectFiles.ContainsKey(path))
                _projectFiles.Remove(path);
            textBuffer.Changed -= TextBufferChanged;
        }

        private async void TextBufferChanged(object sender, TextContentChangedEventArgs e)
        {
            AnalysisEntry entry = _projectFiles[e.After.TextBuffer.GetFilePath()];
            await entry.UpdateModel(e.After.GetText());
        }

        internal AnalysisEntry CreateProjectEntry(ITextBuffer textBuffer)
        {
            string path = textBuffer.GetFilePath();
            Random random = new Random();
            int id;
            do
            {
                id = random.Next(1000);     // TODO: use a more proper number
            } while (_projectFilesById.ContainsKey(id));

            _projectBufferById[id] = textBuffer;    // TODO: move to a better place

            AnalysisEntry entry = new AnalysisEntry(this, path, id);
            _projectFiles[path] = entry;
            _projectFilesById[id] = entry;
            return entry;
        }

        internal AnalysisEntry GetAnalysisEntryFromPath(string path)
        {
            AnalysisEntry res;
            if (_projectFiles.TryGetValue(path, out res))
            {
                return res;
            }
            return null;
        }

        internal ITextBuffer GetTextBufferById(int id)
        {
            ITextBuffer buffer;
            if (_projectBufferById.TryGetValue(id, out buffer))
            {
                return buffer;
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="textView"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        internal SourceSpan? GetDeclarationLocation(AnalysisEntry entry, ITextView textView, SnapshotPoint point)
        {
            int spanStart, spanEnd;
            for (spanEnd = point.Position; spanEnd < point.Snapshot.Length; ++spanEnd)
            {
                if (point.Snapshot.GetText().Substring(spanEnd, 1).ToLower().ToCharArray()[0] < 'a' ||
                    point.Snapshot.GetText().Substring(spanEnd, 1).ToLower().ToCharArray()[0] > 'z')
                    break;
            }

            for (spanStart = point.Position; spanStart > 0; --spanStart)
            {
                if (point.Snapshot.GetText().Substring(spanStart - 1, 1).ToLower().ToCharArray()[0] < 'a' ||
                    point.Snapshot.GetText().Substring(spanStart - 1, 1).ToLower().ToCharArray()[0] > 'z')
                    break;
            }

            string word = point.Snapshot.GetText().Substring(spanStart, spanEnd - spanStart);
            SourceSpan span = new SourceSpan(spanStart, point.GetContainingLine().LineNumber, spanEnd, point.GetContainingLine().LineNumber);

            SourceSpan? res = entry.Model.GetDeclarationLocation(word, span);

            return res;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="textBuffer"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        internal string GetDescription(AnalysisEntry entry, ITextBuffer textBuffer, SnapshotPoint point)
        {
            int spanStart, spanEnd;
            for (spanEnd = point.Position; spanEnd < point.Snapshot.Length; ++spanEnd)
            {
                if (point.Snapshot.GetText().Substring(spanEnd, 1).ToLower().ToCharArray()[0] < 'a' ||
                    point.Snapshot.GetText().Substring(spanEnd, 1).ToLower().ToCharArray()[0] > 'z')
                    break;
            }

            for (spanStart = point.Position; spanStart > 0; --spanStart)
            {
                if (point.Snapshot.GetText().Substring(spanStart - 1, 1).ToLower().ToCharArray()[0] < 'a' ||
                    point.Snapshot.GetText().Substring(spanStart - 1, 1).ToLower().ToCharArray()[0] > 'z')
                    break;
            }
            string word = point.Snapshot.GetText().Substring(spanStart, spanEnd - spanStart);
            SourceSpan span = new SourceSpan(spanStart, point.GetContainingLine().LineNumber, spanEnd, point.GetContainingLine().LineNumber);

            SourceSpan? res = entry.Model.GetDeclarationLocation(word, span);

            if (!res.HasValue)
                return null;

            string description = String.Empty;
            Irony.Parsing.Scanner scanner = new Irony.Parsing.Parser(IronyParser.LuaGrammar.Instance).Scanner;
            int funcDefLine = res.Value.StartLine;
            for (int i = funcDefLine - 1; i >= 0; --i)
            {
                string lineText = textBuffer.CurrentSnapshot.GetLineFromLineNumber(i).GetText();
                int state = 0;
                scanner.VsSetSource(lineText, 0);
                Irony.Parsing.Token token = scanner.VsReadToken(ref state);
                if (token == null || token.Terminal.Name != "block-comment")
                    break;
                if (token.Terminal.Name == "block-comment")
                    description = (description == String.Empty) ? token.ValueString : token.ValueString + "\n" + description;
            }

            return description;
        }

        internal List<Region> GetOutliningRegions(AnalysisEntry entry)
        {
            return entry.Model.GetOutliningRegions();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="textView"></param>
        internal void FormatBlock(AnalysisEntry entry, ITextView textView)
        {
            int startLine = textView.Selection.Start.Position.GetContainingLine().LineNumber;
            int endLine = textView.Selection.End.Position.GetContainingLine().LineNumber;

            ITextSnapshot snapshot = textView.TextSnapshot;

            int[] indentations;
            entry.Model.RetrieveIndentationsFromSyntaxTree(out indentations);

            // Need a new parser and scanner here
            Irony.Parsing.Parser parser = new Irony.Parsing.Parser(IronyParser.LuaGrammar.Instance);
            Irony.Parsing.Scanner scanner = parser.Scanner;

            //rule 1: insert space before and after binary operator if there not any
            //rule 2: insert space after comma, semicolon if there not any
            //rule 3: indentation increase inside block
            //rule 4: multiple spaces replaced by a single space
            using (var edit = textView.TextBuffer.CreateEdit())
            {
                //IEnumerable<ITextSnapshotLine> lines = view.TextSnapshot.Lines;
                for (int lineNumber = startLine; lineNumber <= endLine; lineNumber++)
                {
                    ITextSnapshotLine line = snapshot.GetLineFromLineNumber(lineNumber);
                    int lineOffset = line.Start.Position;
                    string lineText = line.GetText();

                    scanner.VsSetSource(lineText, 0);

                    int state = 0;
                    Irony.Parsing.Token currentToken = scanner.VsReadToken(ref state);
                    Irony.Parsing.Token lastToken = null;
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
                        Irony.Parsing.Token nextToken = scanner.VsReadToken(ref state);
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
        }
    }
}
