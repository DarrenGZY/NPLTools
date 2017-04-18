using Microsoft.VisualStudio.Text;
using NPLTools.IronyParser.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLTools.Intelligense
{
    // helper class to parse the content
    internal static class ReverseParser
    {
        internal static IdentifierSource ParseIdentifier(SnapshotPoint point)
        {
            int spanStart, spanEnd;
            for (spanEnd = point.Position; spanEnd < point.Snapshot.Length; ++spanEnd)
            {
                if (!char.IsLetterOrDigit(point.Snapshot[spanEnd]))
                    break;
            }

            for (spanStart = point.Position; spanStart > 0; --spanStart)
            {
                if (!char.IsLetterOrDigit(point.Snapshot[spanStart - 1]) && 
                    point.Snapshot[spanStart - 1] != '.' &&
                    point.Snapshot[spanStart - 1] != ':')
                    break;
            }
            string id = point.Snapshot.GetText().Substring(spanStart, spanEnd - spanStart);
            ScopeSpan span = new ScopeSpan(spanStart, point.GetContainingLine().LineNumber, spanEnd, point.GetContainingLine().LineNumber);
            return new IdentifierSource(id, span);
        } 
    }

    internal struct IdentifierSource
    {
        public string Identifier;
        public ScopeSpan Span;
        public List<IdentifierSource> NameSpaces;

        public IdentifierSource(string id, ScopeSpan span, List<IdentifierSource> nameSpaces)
        {
            Identifier = id;
            Span = span;
            NameSpaces = nameSpaces;
        }

        public IdentifierSource(string id, ScopeSpan span)
        {
            Identifier = id;
            Span = span;
            NameSpaces = null;
        }
    }

}
