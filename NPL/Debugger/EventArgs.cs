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

    public class ModuleLoadEventArgs : EventArgs
    {
        private LuaModule _module;

        public ModuleLoadEventArgs(LuaModule module)
        {
            _module = module;
        }

        public LuaModule Module => _module;
    }

    public class FrameListEventArgs : EventArgs
    {
        private IList<LuaStackFrame> _frames;

        public FrameListEventArgs(IList<LuaStackFrame> frames)
        {
            _frames = frames;
        }

        public IList<LuaStackFrame> Frames => _frames;
    }
}
