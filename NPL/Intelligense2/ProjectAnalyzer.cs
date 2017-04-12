using Microsoft.VisualStudio.Text;
using NPLTools.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPLTools.Language;
using Microsoft.VisualStudio.Text.Editor;

namespace NPLTools.Intelligense2
{
    public class ProjectAnalyzer
    {
        // 
        private Dictionary<string, AnalysisEntry> _projectFiles = new Dictionary<string, AnalysisEntry>();
        private Dictionary<int, AnalysisEntry> _projectFilesById = new Dictionary<int, AnalysisEntry>();
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

        internal void MonitorTextBuffer(ITextBuffer textBuffer)
        {
            AnalysisEntry entry = CreateProjectEntry(textBuffer);
            _entryService.SetAnalyzer(textBuffer, this);
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

        internal ITrackingSpan GetDeclarationLocation(AnalysisEntry entry, ITextView textView, SnapshotPoint point)
        {
            string w = point.Snapshot.GetText().Substring(point.Position, 1);
            int spanStart, spanEnd;
            for (spanEnd = point.Position; spanEnd < point.Snapshot.Length; ++spanEnd)
            {
                if (point.Snapshot.GetText().Substring(spanEnd, 1).ToLower().ToCharArray()[0] < 'a' ||
                    point.Snapshot.GetText().Substring(spanEnd, 1).ToLower().ToCharArray()[0] > 'z')
                    break;
            }

            for (spanStart = point.Position - 1; spanStart >= 0; --spanStart)
            {
                if (point.Snapshot.GetText().Substring(spanStart, 1).ToLower().ToCharArray()[0] < 'a' ||
                    point.Snapshot.GetText().Substring(spanStart, 1).ToLower().ToCharArray()[0] > 'z')
                    break;
            }

            string word = point.Snapshot.GetText().Substring(spanStart, spanEnd - spanStart);

            return null;
        }
    }
}
