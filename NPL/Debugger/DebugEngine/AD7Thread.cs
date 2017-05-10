using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.Debugger.Interop;
using System.Diagnostics;

namespace NPLTools.Debugger.DebugEngine
{
    // This class implements IDebugThread2 which represents a thread running in a program.
    class AD7Thread : IDebugThread2, IDebugThread100
    {
        int IDebugThread100.CanDoFuncEval()
        {
            throw new NotImplementedException();
        }

        int IDebugThread2.CanSetNextStatement(IDebugStackFrame2 pStackFrame, IDebugCodeContext2 pCodeContext)
        {
            throw new NotImplementedException();
        }

        int IDebugThread2.EnumFrameInfo(enum_FRAMEINFO_FLAGS dwFieldSpec, uint nRadix, out IEnumDebugFrameInfo2 ppEnum)
        {
            throw new NotImplementedException();
        }

        int IDebugThread100.GetFlags(out uint pFlags)
        {
            throw new NotImplementedException();
        }

        int IDebugThread2.GetLogicalThread(IDebugStackFrame2 pStackFrame, out IDebugLogicalThread2 ppLogicalThread)
        {
            throw new NotImplementedException();
        }

        int IDebugThread2.GetName(out string pbstrName)
        {
            throw new NotImplementedException();
        }

        int IDebugThread2.GetProgram(out IDebugProgram2 ppProgram)
        {
            throw new NotImplementedException();
        }

        int IDebugThread100.GetThreadDisplayName(out string bstrDisplayName)
        {
            throw new NotImplementedException();
        }

        int IDebugThread2.GetThreadId(out uint pdwThreadId)
        {
            throw new NotImplementedException();
        }

        int IDebugThread2.GetThreadProperties(enum_THREADPROPERTY_FIELDS dwFields, THREADPROPERTIES[] ptp)
        {
            throw new NotImplementedException();
        }

        int IDebugThread100.GetThreadProperties100(uint dwFields, THREADPROPERTIES100[] ptp)
        {
            throw new NotImplementedException();
        }

        int IDebugThread2.Resume(out uint pdwSuspendCount)
        {
            throw new NotImplementedException();
        }

        int IDebugThread100.SetFlags(uint flags)
        {
            throw new NotImplementedException();
        }

        int IDebugThread2.SetNextStatement(IDebugStackFrame2 pStackFrame, IDebugCodeContext2 pCodeContext)
        {
            throw new NotImplementedException();
        }

        int IDebugThread100.SetThreadDisplayName(string bstrDisplayName)
        {
            throw new NotImplementedException();
        }

        int IDebugThread2.SetThreadName(string pszName)
        {
            throw new NotImplementedException();
        }

        int IDebugThread2.Suspend(out uint pdwSuspendCount)
        {
            throw new NotImplementedException();
        }
    }
}
