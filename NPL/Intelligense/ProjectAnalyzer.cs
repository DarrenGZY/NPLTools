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
using NPLTools.IronyParser.Ast;
using System.IO;

namespace NPLTools.Intelligense
{
    public class ProjectAnalyzer
    {
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
            AnalysisEntry entry;
            if (_projectFiles.ContainsKey(textBuffer.GetFilePath()))
                entry = _projectFiles[textBuffer.GetFilePath()];
            else
                entry = CreateAnalysisEntry(textBuffer);
            _entryService.SetAnalyzer(textBuffer, this);
            textBuffer.Changed += TextBufferChanged;
            return entry;
        }

        internal bool HasMonitoredTextBuffer(ITextBuffer textBuffer)
        {
            return textBuffer.GetAnalysisAtCaret(_serviceProvider) != null;
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

        internal AnalysisEntry CreateAnalysisEntry(ITextBuffer textBuffer)
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

        internal AnalysisEntry CreateAnalysisEntry(string path)
        {
            Random random = new Random();
            int id;
            do
            {
                id = random.Next(1000);     // TODO: use a more proper number
            } while (_projectFilesById.ContainsKey(id));

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

        private readonly object myLock = new object();
        internal SynchronizedCollection<AnalysisEntry> GetAnalysisEntries()
        {
            return new SynchronizedCollection<AnalysisEntry>(myLock, _projectFiles.Values);
        }

        /// <summary>
        /// Get the location where declaration settles
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="textView"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        internal KeyValuePair<string, ScopeSpan>? GetDeclarationLocation(AnalysisEntry entry, ITextView textView, SnapshotPoint point)
        {
            IdentifierSource word = ReverseParser.ParseIdentifier(point);

            // Find declaration in its own file
            ScopeSpan? res = entry.Model.GetDeclarationLocation(word.Identifier, word.Span);
            if (res != null)
                return new KeyValuePair<string, ScopeSpan>(entry.FilePath, res.Value);
            // Find declaration in files in project
            var pair = GetDeclarationinFiles(word.Identifier);
            return pair;
        }

        private KeyValuePair<string, ScopeSpan>? GetDeclarationinFiles(string name)
        {
            foreach (var path in _projectFiles.Keys)
            {
                var entry = _projectFiles[path];
                ScopeSpan? res = entry.Model.GetGlobalDeclarationLocation(name);
                if (res != null)
                    return new KeyValuePair<string, ScopeSpan>(path, res.Value);
            }
            return null;
        }

        /// <summary>
        /// Get the description of a defined function
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="textBuffer"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        internal string GetDescription(AnalysisEntry entry, ITextBuffer textBuffer, SnapshotPoint point)
        {
            string description = String.Empty;
            IdentifierSource word = ReverseParser.ParseIdentifier(point);
            // Try to get description in the file
            ScopeSpan? inFileDeclaration = entry.Model.GetDeclarationLocation(word.Identifier, word.Span);
            if (inFileDeclaration.HasValue)
            {
                Irony.Parsing.Scanner scanner = new Irony.Parsing.Parser(IronyParser.LuaGrammar.Instance).Scanner;
                int funcDefLine = inFileDeclaration.Value.StartLine;
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
            // Not find declaration in the file, try to get description from loaded file 
            var loadedFileDeclaration = GetDeclarationinFiles(word.Identifier);
            if (loadedFileDeclaration.HasValue)
            {
                ScopeSpan declarationScope = loadedFileDeclaration.Value.Value;
                string declarationFile = loadedFileDeclaration.Value.Key;
                try
                {
                    Irony.Parsing.Scanner scanner = new Irony.Parsing.Parser(IronyParser.LuaGrammar.Instance).Scanner;
                    int funcDefLine = declarationScope.StartLine;
                    string[] lines = File.ReadAllLines(declarationFile);
                    for (int i = funcDefLine - 1; i >= 0; --i)
                    {
                        string lineText = lines[i];
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
                catch (Exception)
                {

                }
            }

            return description;
        }

        internal List<Region> GetOutliningRegions(AnalysisEntry entry)
        {
            return entry.Model.GetOutliningRegions();
        }

        /// <summary>
        /// Format selection block
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="textView"></param>
        internal void FormatBlock(AnalysisEntry entry, ITextView textView)
        {
            int startLine = textView.Selection.Start.Position.GetContainingLine().LineNumber;
            int endLine = textView.Selection.End.Position.GetContainingLine().LineNumber;

            ITextSnapshot snapshot = textView.TextSnapshot;

            int[] indentations;
            bool[] fixedLines;
            entry.Model.RetrieveIndentationsFromSyntaxTree(out indentations, out fixedLines);

            //Get long-string and block-comment spans, which are not to be added or removed indentations 


            //Need a new parser and scanner here
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
                    if (fixedLines[lineNumber]) continue;

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

                    while (currentToken != null && currentToken.Terminal.Name != "SYNTAX_ERROR")
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
