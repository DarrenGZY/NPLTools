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
        private LuaThread _thread;

        public BreakpointEventArgs(LuaThread thread, LuaBreakpoint breakpoint)
        {
            _breakpoint = breakpoint;
        }

        public LuaBreakpoint Breakpoint => _breakpoint;
        public LuaThread Thread => _thread;
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
        private LuaThread _thread;
        private IList<LuaStackFrame> _frames;

        public FrameListEventArgs(LuaThread thread)
        {
            _thread = thread;
        }

        public LuaThread Thread => _thread;
        public IList<LuaStackFrame> Frames => _frames;
    }

    public class ThreadCreateEventArgs : EventArgs
    {
        private LuaThread _thread;

        public ThreadCreateEventArgs(LuaThread thread)
        {
            _thread = thread;
        }

        public LuaThread Thread => _thread;
    }

    public class ProcessExitedEventArgs : EventArgs
    {
        private readonly int _exitCode;

        public ProcessExitedEventArgs(int exitCode)
        {
            _exitCode = exitCode;
        }

        public int ExitCode => _exitCode;
    }
}
