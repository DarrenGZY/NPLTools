using System;
using Microsoft.VisualStudio.Debugger.Interop;
using System.Collections.Generic;
using System.Linq;
namespace NPLTools.Debugger.DebugEngine
{
    // This class manages breakpoints for the engine. 
    public class BreakpointManager
    {
        private AD7Engine m_engine;
        private System.Collections.Generic.List<AD7PendingBreakpoint> m_pendingBreakpoints;
        private readonly Dictionary<LuaBreakpoint, AD7BoundBreakpoint> _breakpointMap = new Dictionary<LuaBreakpoint, AD7BoundBreakpoint>();

        public BreakpointManager(AD7Engine engine)
        {
            m_engine = engine;
            m_pendingBreakpoints = new System.Collections.Generic.List<AD7PendingBreakpoint>();
        }
      
        // A helper method used to construct a new pending breakpoint.
        public void CreatePendingBreakpoint(IDebugBreakpointRequest2 pBPRequest, out IDebugPendingBreakpoint2 ppPendingBP)
        {
            AD7PendingBreakpoint pendingBreakpoint = new AD7PendingBreakpoint(pBPRequest, m_engine, this);
            ppPendingBP = (IDebugPendingBreakpoint2)pendingBreakpoint;
            m_pendingBreakpoints.Add(pendingBreakpoint);
        }

        // Called from the engine's detach method to remove the debugger's breakpoint instructions.
        public void ClearBoundBreakpoints()
        {
            foreach (AD7PendingBreakpoint pendingBreakpoint in m_pendingBreakpoints)
            {
                pendingBreakpoint.ClearBoundBreakpoints();
            }
        }

        public void AddBoundBreakpoint(LuaBreakpoint breakpoint, AD7BoundBreakpoint boundBreakpoint)
        {
            _breakpointMap[breakpoint] = boundBreakpoint;
        }

        public void RemoveBoundBreakpoint(LuaBreakpoint breakpoint)
        {
            _breakpointMap.Remove(breakpoint);
        }

        public AD7BoundBreakpoint GetBreakpoint(LuaBreakpoint breakpoint)
        {
            return _breakpointMap[breakpoint];
        }

        public AD7BoundBreakpoint GetBreakpoint()
        {
            return _breakpointMap.First().Value;
        }
    }
}
