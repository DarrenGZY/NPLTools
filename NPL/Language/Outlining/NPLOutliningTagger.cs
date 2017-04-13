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
        private ITextBuffer _buffer;
        private AnalysisEntry _analysisEntry;
        private List<Region> _regions;

        public NPLOutliningTagger(NPLOutliningTaggerProvider provider, ITextBuffer buffer)
        {
            _buffer = buffer;
            _provider = provider;
            _analysisEntry = _buffer.GetAnalysisAtCaret(provider.ServiceProvider);
            _regions = new List<Region>();
            ReParse();
            _buffer.Changed += BufferChanged;
        }

        public IEnumerable<ITagSpan<IOutliningRegionTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0)
                yield break;

            foreach (var region in _regions)
            {
                if (region.Start <= spans[spans.Count-1].End &&
                    (region.Start+region.Length) >= spans[0].Start)
                {
                    yield return new TagSpan<IOutliningRegionTag>(
                        new SnapshotSpan(spans[0].Snapshot, region.Start, region.Length ),
                        new OutliningRegionTag(false, false, "...", spans[0].Snapshot.GetText().Substring(region.Start, region.Length)));
                }
            }
        }

        void BufferChanged(object sender, TextContentChangedEventArgs e)
        {
            ReParse();
        }

        private void ReParse()
        {
            _regions = _analysisEntry.Analyzer.GetOutliningRegions(_analysisEntry);
            if (TagsChanged != null)
                TagsChanged(this, new SnapshotSpanEventArgs(new SnapshotSpan(_buffer.CurrentSnapshot, 0, _buffer.CurrentSnapshot.Length)));
        }
    }
}
