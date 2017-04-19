using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text;
using NPLTools.IronyParser;
using Irony.Parsing;
using Microsoft.VisualStudio.Shell;
using NPLTools.Intelligense;
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
        private List<Region> _regions;

        public NPLOutliningTagger(NPLOutliningTaggerProvider provider, ITextBuffer textBuffer)
        {
            _textBuffer = textBuffer;
            _provider = provider;
            IServiceProvider serviceProvider = _provider.ServiceProvider as IServiceProvider;
            IVsSolution sln = serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
            var project = sln.GetLoadedProject().GetNPLProject();
            if (!project.GetAnalyzer().HasMonitoredTextBuffer(textBuffer))
                project.GetAnalyzer().MonitorTextBuffer(textBuffer);
            _analysisEntry = _textBuffer.GetAnalysisAtCaret(provider.ServiceProvider);
            _regions = new List<Region>();
            _analysisEntry.NewParseTree += OnNewParseTree;
            //ReParse();
        }

        private void OnNewParseTree(object sender, ParseTreeChangedEventArgs e)
        {
            if (e.Tree != null && e.Tree.Root != null)
            {
                _regions.Clear();
                WalkSyntaxTreeForOutliningRegions(e.Tree.Root, _regions);
            }       
        }

        public IEnumerable<ITagSpan<IOutliningRegionTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0)
                yield break;

            foreach (var region in _regions)
            {
                if (region.Start <= spans[spans.Count - 1].End &&
                    (region.Start + region.Length) >= spans[0].Start)
                {
                    yield return new TagSpan<IOutliningRegionTag>(
                        new SnapshotSpan(spans[0].Snapshot, region.Start, region.Length),
                        new OutliningRegionTag(false, false, "...", spans[0].Snapshot.GetText().Substring(region.Start, region.Length)));
                }
            }
        }

        private Task ReParse()
        {
            return Task.Run(()=> {
                _regions = _analysisEntry.Analyzer.GetOutliningRegions(_analysisEntry);
                if (TagsChanged != null)
                    TagsChanged(this, new SnapshotSpanEventArgs(new SnapshotSpan(_textBuffer.CurrentSnapshot, 0, _textBuffer.CurrentSnapshot.Length)));
            });
        }

        private void WalkSyntaxTreeForOutliningRegions(ParseTreeNode node, List<Region> regions)
        {
            if (node == null) return;

            int startPosition, length;
            if (node.Term.Name == NPLConstants.FunctionDeclaration)
            {
                startPosition = node.ChildNodes[2].Span.EndPosition + 1;
                length = node.ChildNodes[4].Span.Location.Position - startPosition;
                regions.Add(new Region(startPosition, length));
            }
            else if (node.Term.Name == NPLConstants.LocalFunctionDeclaration)
            {
                startPosition = node.ChildNodes[3].Span.EndPosition + 1;
                length = node.ChildNodes[5].Span.Location.Position - startPosition;
                regions.Add(new Region(startPosition, length));
            }
            else if (node.Term.Name == NPLConstants.DoBlock)
            {
                startPosition = node.ChildNodes[0].Span.EndPosition;
                length = node.ChildNodes[2].Span.Location.Position - node.ChildNodes[0].Span.EndPosition;
                regions.Add(new Region(startPosition, length));
            }
            else if (node.Term.Name == NPLConstants.WhileBlock)
            {
                startPosition = node.ChildNodes[0].Span.EndPosition;
                length = node.ChildNodes[4].Span.Location.Position - node.ChildNodes[0].Span.EndPosition;
                regions.Add(new Region(startPosition, length));
            }
            else if (node.Term.Name == NPLConstants.RepeatBlock)
            {
                startPosition = node.ChildNodes[0].Span.EndPosition;
                length = node.ChildNodes[3].Span.EndPosition - node.ChildNodes[0].Span.EndPosition;
                regions.Add(new Region(startPosition, length));
            }
            else if (node.Term.Name == NPLConstants.ForBlock ||
                node.Term.Name == NPLConstants.GenericForBlock ||
                node.Term.Name == NPLConstants.ConditionBlock)
            {
                startPosition = node.ChildNodes[0].Span.EndPosition;
                length = node.ChildNodes[6].Span.Location.Position - node.ChildNodes[0].Span.EndPosition;
                regions.Add(new Region(startPosition, length));
            }

            foreach (ParseTreeNode child in node.ChildNodes)
            {
                WalkSyntaxTreeForOutliningRegions(child, regions);
            }
        }
    }
}
