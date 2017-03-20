//***************************************************************************
// 
//    Copyright (c) Microsoft Corporation. All rights reserved.
//    This code is licensed under the Visual Studio SDK license terms.
//    THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//    ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//    IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//    PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//***************************************************************************

namespace NPL.Classifier
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Text;
    using Microsoft.VisualStudio.Text.Classification;
    using Microsoft.VisualStudio.Text.Editor;
    using Microsoft.VisualStudio.Text.Tagging;
    using Microsoft.VisualStudio.Utilities;

    [Export(typeof(ITaggerProvider))]
    [ContentType("NPL")]
    [TagType(typeof(NPLTokenTag))]
    internal sealed class NPLTokenTagProvider : ITaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            return new NPLTokenTagger(buffer) as ITagger<T>;
        }
    }

    public class NPLTokenTag : ITag 
    {
        public NPLTokenTypes type { get; private set; }

        public NPLTokenTag(NPLTokenTypes type)
        {
            this.type = type;
        }
    }

    internal sealed class NPLTokenTagger : ITagger<NPLTokenTag>
    {

        ITextBuffer _buffer;
        IDictionary<string, NPLTokenTypes> _nplTypes;

        internal NPLTokenTagger(ITextBuffer buffer)
        {
            _buffer = buffer;
            _nplTypes = new Dictionary<string, NPLTokenTypes>();
            _nplTypes["ook!"] = NPLTokenTypes.OokExclamation;
            _nplTypes["ook."] = NPLTokenTypes.OokPeriod;
            _nplTypes["ook?"] = NPLTokenTypes.OokQuestion;
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged
        {
            add { }
            remove { }
        }

        public IEnumerable<ITagSpan<NPLTokenTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {

            foreach (SnapshotSpan curSpan in spans)
            {
                ITextSnapshotLine containingLine = curSpan.Start.GetContainingLine();
                int curLoc = containingLine.Start.Position;
                string[] tokens = containingLine.GetText().ToLower().Split(' ');

                foreach (string nplToken in tokens)
                {
                    if (_nplTypes.ContainsKey(nplToken))
                    {
                        var tokenSpan = new SnapshotSpan(curSpan.Snapshot, new Span(curLoc, nplToken.Length));
                        if( tokenSpan.IntersectsWith(curSpan) ) 
                            yield return new TagSpan<NPLTokenTag>(tokenSpan, 
                                                                  new NPLTokenTag(_nplTypes[nplToken]));
                    }

                    //add an extra char location because of the space
                    curLoc += nplToken.Length + 1;
                }
            }
            
        }
    }
}
