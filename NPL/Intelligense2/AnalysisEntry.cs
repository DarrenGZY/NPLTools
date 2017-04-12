using Irony.Parsing;
using NPLTools.Intelligense;
using NPLTools.IronyParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLTools.Intelligense2
{
    internal sealed class AnalysisEntry
    {
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
        }

        public ProjectAnalyzer Analyzer => _analyzer;

        public LuaModel Model => _model;
    }
}
