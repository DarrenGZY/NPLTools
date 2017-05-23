using Microsoft.VisualStudio.Text;
using NPLTools.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NPLTools.Language;
using SourceSpan = NPLTools.Intellisense.SourceSpan;
using Microsoft.VisualStudio.Text.Editor;
using NPLTools.IronyParser.Ast;
using System.IO;
using Newtonsoft.Json;
using LibGit2Sharp;

namespace NPLTools.Intellisense
{
    public class ProjectAnalyzer
    {
        private Dictionary<string, AnalysisEntry> _projectFiles = new Dictionary<string, AnalysisEntry>();
        private Dictionary<int, AnalysisEntry> _projectFilesById = new Dictionary<int, AnalysisEntry>();
        private Dictionary<int, ITextBuffer> _projectBufferById = new Dictionary<int, ITextBuffer>();
        private readonly IServiceProvider _serviceProvider;
        private readonly AnalysisEntryService _entryService;
        private HashSet<Declaration> _predefinedDeclarations = new HashSet<Declaration>();

        public ProjectAnalyzer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _entryService = _serviceProvider.GetEntryService();
        }

        public bool ContainsFile(string filePath)
        {
            return _projectFiles.ContainsKey(filePath);
        }

        public AnalysisEntry GetAnalysisEntry(string filePath)
        {
            return _projectFiles[filePath];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="textBuffer"></param>
        /// <returns></returns>
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
            return textBuffer.GetAnalysisAtCaretProjectMode(_serviceProvider) != null;
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

        /// <summary>
        /// Use SynchronizedCollection to be thread-safe
        /// </summary>
        /// <returns></returns>
        internal SynchronizedCollection<AnalysisEntry> GetAnalysisEntries()
        {
            return new SynchronizedCollection<AnalysisEntry>(myLock, _projectFiles.Values);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlPath"></param>
        internal void AddPredefinedDeclarationsFromXML(string xmlPath)
        {
            _predefinedDeclarations.UnionWith(XmlDocumentationLoader.LoadXml(xmlPath));
        }

        internal async void AnalyzeJson(string jsonPath, string workingDir)
        {
            using (StreamReader sr = File.OpenText(jsonPath))
            {
                string jsonContent = sr.ReadToEnd();
                PackageJson json = JsonConvert.DeserializeObject<PackageJson>(jsonContent);

                foreach (var dependency in json.Dependencies)
                {
                    string downloadLink = String.Empty;
                    switch (dependency.Key)
                    {
                        case "main":
                            downloadLink = @"https://github.com/NPLPackages/main.git";
                            break;
                        case "mime":
                            downloadLink = @"https://github.com/caoyongfeng0214/nplmime.git";
                            break;
                        case "express":
                            downloadLink = @"https://github.com/caoyongfeng0214/nplexpress.git";
                            break;
                        case "lustache":
                            downloadLink = @"https://github.com/caoyongfeng0214/npllustache.git";
                            break;
                        case "common":
                            downloadLink = @"https://github.com/caoyongfeng0214/nplcommon.git";
                            break;
                        default:
                            break;
                    }

                    if (downloadLink != String.Empty)
                    {
                        string dependencyPath = Path.Combine(workingDir, "npl_packages", dependency.Key);
                        if (!Directory.Exists(dependencyPath))
                        {
                            await Task.Run(() => { Repository.Clone(downloadLink, dependencyPath); });
                        }
                    }
                }
            }
        }

        internal Declaration GetDeclarationFromPredeined(string name)
        {
            Declaration declaration = DeclarationHelper.BuildDeclaration(name);
            var founded = _predefinedDeclarations.Where((defined) => defined.Equal(declaration));
            if (founded.Count() != 0)
                return founded.First();
            return null;
        }

        internal void GetCompletionSourceFromPredefined(HashSet<Declaration> res)
        {
            var founded = _predefinedDeclarations;
            res.UnionWith(founded);
        }

    }
}
