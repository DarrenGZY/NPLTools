using Irony.Parsing;
using NPLTools.Intelligense;
using NPLTools.IronyParser;
using NPLTools.IronyParser.Ast;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLTools.Intelligense
{
    internal sealed class AnalysisEntry
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

        public async void InitModel()
        {
            await Task.Run(()=> {
                string source = File.ReadAllText(_path);
                ParseTree parseTree = _parser.Parse(source);
                _model = new LuaModel(parseTree);

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
    }

    internal class ParseTreeChangedEventArgs : EventArgs
    {
        public ParseTree Tree { get; set; }

        public ParseTreeChangedEventArgs(ParseTree tree)
        {
            Tree = tree;
        }
    }
}
