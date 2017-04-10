﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text;
using NPLTools.IronyParser;
using Irony.Parsing;

namespace NPLTools.Language.Outlining
{
    [Export(typeof(ITaggerProvider))]
    [TagType(typeof(IOutliningRegionTag))]
    [ContentType("NPL")]
    internal sealed class OutliningTaggerProvide : ITaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            Func<ITagger<T>> sc = delegate () { return new OutliningTagger(buffer) as ITagger<T>; };
            return buffer.Properties.GetOrCreateSingletonProperty<ITagger<T>>(sc);
        }
    }

    internal sealed class OutliningTagger : ITagger<IOutliningRegionTag>
    {
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        private ITextBuffer _buffer;
        private ITextSnapshot _snapshot;
        private Parser _parser;
        private List<Region> _regions;
        private System.Threading.Timer _delayRefreshTimer;

        public OutliningTagger(ITextBuffer buffer)
        {
            _buffer = buffer;
            _snapshot = buffer.CurrentSnapshot;
            _regions = new List<Region>();
            _parser = new Parser(LuaGrammar.Instance);
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
            if (e.After != _buffer.CurrentSnapshot)
                return;

            if (_delayRefreshTimer != null)
            {
                _delayRefreshTimer.Dispose();
            }
            _delayRefreshTimer = new System.Threading.Timer(ReParseCallBack, null, 500, System.Threading.Timeout.Infinite);
        }

        private void ReParseCallBack(object args)
        {
            ReParse();
        }

        private void ReParse()
        {
            ParseTree tree = _parser.Parse(_buffer.CurrentSnapshot.GetText());
            if (tree.Root == null) return;
            _regions.Clear();
            IterateTreeNode(tree.Root, _regions);
            if (TagsChanged != null)
                TagsChanged(this, new SnapshotSpanEventArgs(new SnapshotSpan(_buffer.CurrentSnapshot, 0, _buffer.CurrentSnapshot.Length)));
        }

        private void IterateTreeNode(ParseTreeNode node, List<Region> regions)
        {
            if (node == null) return;

            int startPosition, length;
            if (node.Term.Name == NPLConstants.FunctionDeclaration)
            {
                startPosition = node.ChildNodes[3].Span.Location.Position;
                length = node.Span.Length - (node.ChildNodes[3].Span.Location.Position - node.Span.Location.Position);
                regions.Add(new Region(startPosition, length));
            }
            else if (node.Term.Name == NPLConstants.LocalFunctionDeclaration)
            {
                startPosition = node.ChildNodes[4].Span.Location.Position;
                length = node.Span.Length - (node.ChildNodes[4].Span.Location.Position - node.Span.Location.Position);
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
                IterateTreeNode(child, regions);
            }
        }
    }

    internal class Region {
        public int Start;
        public int Length;
        public Region(int start, int length)
        {
            this.Start = start;
            this.Length = length;
        }
    }
}
