using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLTools.Intellisense
{
    public struct SourceSpan
    {
        public int StartPosition;
        public int StartLine;
        public int EndPosition;
        public int EndLine;
        public SourceSpan(int startPosition, int startLine, int endPosition, int endLine)
        {
            StartPosition = startPosition;
            StartLine = startLine;
            EndPosition = endPosition;
            EndLine = endLine;
        }
    }

    public struct Region
    {
        public int Start;
        public int Length;
        public Region(int start, int length)
        {
            this.Start = start;
            this.Length = length;
        }
    }
}
