using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLTools.Debugger
{
    public class LuaStackFrame
    {
        private string _filename;
        private int _lineNo;

        public LuaStackFrame(string filename, int lineNo)
        {
            _filename = filename;
            _lineNo = lineNo;
        }

        public string FileName => _filename;
        public int LineNo => _lineNo;
    }

    public class Frame
    {
        public int LineNo;
        public string FileName;
        public Frame Parent;
    }
}
