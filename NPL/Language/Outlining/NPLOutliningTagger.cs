using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text;
using NPLTools.IronyParser;
using Irony.Parsing;
using Microsoft.VisualStudio.Shell;
using NPLTools.Intellisense;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace NPLTools.Language.Outlining
{
    [Export(typeof(ITaggerProvider))]
    [TagType(typeof(IOutliningRegionTag))]
    [ContentType("NPL")]
    internal sealed class NPLOutliningTaggerProvider : ITaggerProvider
    {
        [Import]
        internal SVsServiceProvider ServiceProvider { get; private set; }

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            Func<ITagger<T>> sc = delegate () { return new NPLOutliningTagger(this, buffer) as ITagger<T>; };
            return buffer.Properties.GetOrCreateSingletonProperty<ITagger<T>>(sc);
        }
    }

    internal sealed class NPLOutliningTagger : ITagger<IOutliningRegionTag>
    {
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        private NPLOutliningTaggerProvider _provider;
        private ITextBuffer _textBuffer;
        private AnalysisEntry _analysisEntry;
        private List<ITrackingSpan> _regions;

        public NPLOutliningTagger(NPLOutliningTaggerProvider provider, ITextBuffer textBuffer)
        {
            _textBuffer = textBuffer;
            _provider = provider;
            _regions = new List<ITrackingSpan>();
            IServiceProvider serviceProvider = _provider.ServiceProvider as IServiceProvider;
            IVsSolution sln = serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
            IVsProject proj = sln.GetLoadedProject();
            if (proj == null)
            {
                if (!_textBuffer.Properties.TryGetProperty(typeof(AnalysisEntry), out _analysisEntry))
                {
                    _analysisEntry = new AnalysisEntry(_textBuffer.GetFilePath());
                    _textBuffer.Properties[typeof(AnalysisEntry)] = _analysisEntry;  
                }
                _analysisEntry.NewParseTree += OnNewParseTree;
            }
            else
            {
                var project = sln.GetLoadedProject().GetNPLProject();
                if (!project.Analyzer.HasMonitoredTextBuffer(textBuffer))
                    project.Analyzer.MonitorTextBuffer(textBuffer);
                _analysisEntry = _textBuffer.GetAnalysisAtCaret(provider.ServiceProvider);
                _analysisEntry.NewParseTree += OnNewParseTree;
                _analysisEntry.InitModel();
            }
        }

        private void OnNewParseTree(object sender, ParseTreeChangedEventArgs e)
        {
            if (e.Tree != null && e.Tree.Root != null)
            {
                _regions.Clear();
                WalkSyntaxTreeForOutliningRegions(e.Tree.Root, _regions);
            }
            if (TagsChanged != null)
                TagsChanged(this, new SnapshotSpanEventArgs(new SnapshotSpan(_textBuffer.CurrentSnapshot, 0, _textBuffer.CurrentSnapshot.Length)));
        }

        public IEnumerable<ITagSpan<IOutliningRegionTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0)
                yield break;

            foreach (var region in _regions)
            {
                SnapshotSpan curRegion = region.GetSpan(_textBuffer.CurrentSnapshot);
                if (curRegion.Start <= spans[spans.Count - 1].End &&
                    (curRegion.Start + curRegion.Length) >= spans[0].Start)
                {
                    yield return new TagSpan<IOutliningRegionTag>(
                        curRegion,
                        new OutliningRegionTag(false, false, "...", spans[0].Snapshot.GetText().Substring(curRegion.Start, curRegion.Length)));
                }
            }
        }

        private void WalkSyntaxTreeForOutliningRegions(ParseTreeNode node, List<ITrackingSpan> regions)
        {
            if (node == null) return;

            int startPosition = -1, length = 0;
            if (node.Term.Name == NPLConstants.FunctionDeclaration)
            {
                startPosition = node.ChildNodes[2].Span.EndPosition + 1;
                length = node.ChildNodes[4].Span.Location.Position - startPosition; 
            }
            else if (node.Term.Name == NPLConstants.LocalFunctionDeclaration)
            {
                startPosition = node.ChildNodes[3].Span.EndPosition + 1;
                length = node.ChildNodes[5].Span.Location.Position - startPosition;
            }
            else if (node.Term.Name == NPLConstants.DoBlock)
            {
                startPosition = node.ChildNodes[0].Span.EndPosition;
                length = node.ChildNodes[2].Span.Location.Position - node.ChildNodes[0].Span.EndPosition;
            }
            else if (node.Term.Name == NPLConstants.WhileBlock)
            {
                startPosition = node.ChildNodes[0].Span.EndPosition;
                length = node.ChildNodes[4].Span.Location.Position - node.ChildNodes[0].Span.EndPosition;
            }
            else if (node.Term.Name == NPLConstants.RepeatBlock)
            {
                startPosition = node.ChildNodes[0].Span.EndPosition;
                length = node.ChildNodes[3].Span.EndPosition - node.ChildNodes[0].Span.EndPosition;
            }
            else if (node.Term.Name == NPLConstants.ForBlock ||
                node.Term.Name == NPLConstants.GenericForBlock ||
                node.Term.Name == NPLConstants.ConditionBlock)
            {
                startPosition = node.ChildNodes[0].Span.EndPosition;
                length = node.ChildNodes[6].Span.Location.Position - node.ChildNodes[0].Span.EndPosition;
            }

            if (startPosition != -1 && length != 0)
                regions.Add(_textBuffer.CurrentSnapshot.CreateTrackingSpan(new Span(startPosition, length), SpanTrackingMode.EdgeExclusive));

            foreach (ParseTreeNode child in node.ChildNodes)
            {
                WalkSyntaxTreeForOutliningRegions(child, regions);
            }
        }
    }
}
