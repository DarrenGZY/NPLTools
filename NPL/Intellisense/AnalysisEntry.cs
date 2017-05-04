using Irony.Parsing;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using NPLTools.Intellisense;
using NPLTools.IronyParser;
using NPLTools.IronyParser.Ast;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLTools.Intellisense
{
    public sealed class AnalysisEntry
    {
        public event EventHandler<ParseTreeChangedEventArgs> NewParseTree;

        private readonly int _fileId;
        private readonly string _path;
        private readonly ProjectAnalyzer _analyzer;
        private LuaModel _model;
        private readonly Parser _parser;

        public AnalysisEntry(ProjectAnalyzer analyzer, string path, int fileId)
        {
            _analyzer = analyzer;
            _path = path;
            _fileId = fileId;
            _parser = new Parser(LuaGrammar.Instance);
            InitModel();
        }

        public AnalysisEntry(string path)
        {
            _path = path;
            _parser = new Parser(LuaGrammar.Instance);
            InitModel();
        }

        public async void InitModel()
        {
            await Task.Run(()=> {
                string source = File.ReadAllText(_path);
                ParseTree parseTree = _parser.Parse(source);
                _model = new LuaModel(parseTree, this);

                if (NewParseTree != null)
                    NewParseTree(this, new ParseTreeChangedEventArgs(parseTree));
            });
        }

        public Task UpdateModel(string source)
        {
            return Task.Run(() =>
            {
                ParseTree parseTree = _parser.Parse(source);

                if (parseTree.Root != null)
                    _model.Update(parseTree);

                if (NewParseTree != null)
                    NewParseTree(this, new ParseTreeChangedEventArgs(parseTree));
            });
        }
        
        public string FilePath => _path;

        public ProjectAnalyzer Analyzer => _analyzer;

        public LuaModel Model => _model;

        /// <summary>
        /// Format selection block
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="textView"></param>
        internal void FormatBlock(ITextView textView)
        {
            int startLine = textView.Selection.Start.Position.GetContainingLine().LineNumber;
            int endLine = textView.Selection.End.Position.GetContainingLine().LineNumber;

            ITextSnapshot snapshot = textView.TextSnapshot;

            int[] indentations;
            bool[] fixedLines;
            this.Model.RetrieveIndentationsFromSyntaxTree(out indentations, out fixedLines);

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

    public class ParseTreeChangedEventArgs : EventArgs
    {
        public ParseTree Tree { get; set; }

        public ParseTreeChangedEventArgs(ParseTree tree)
        {
            Tree = tree;
        }
    }
}
