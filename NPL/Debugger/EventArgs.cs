using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLTools.Debugger
{
    public class BreakpointEventArgs : EventArgs
    {
        private LuaBreakpoint _breakpoint;

        public BreakpointEventArgs(LuaBreakpoint breakpoint)
        {
            _breakpoint = breakpoint;
        }

        public LuaBreakpoint Breakpoint => _breakpoint;
    }
}
