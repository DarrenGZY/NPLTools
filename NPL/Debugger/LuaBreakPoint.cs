// Python Tools for Visual Studio
// Copyright(c) Microsoft Corporation
// All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the License); you may not use
// this file except in compliance with the License. You may obtain a copy of the
// License at http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED ON AN  *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS
// OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY
// IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE,
// MERCHANTABLITY OR NON-INFRINGEMENT.
//
// See the Apache Version 2.0 License for specific language governing
// permissions and limitations under the License.

using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NPLTools.Debugger
{
    // Must be in sync with BREAKPOINT_CONDITION_* constants in visualstudio_py_debugger.py.
    public enum LuaBreakpointConditionKind
    {
        Always = 0,
        WhenTrue = 1,
        WhenChanged = 2
    }

    // Must be in sync with BREAKPOINT_PASS_COUNT_* constants in visualstudio_py_debugger.py.
    public enum LuaBreakpointPassCountKind
    {
        Always = 0,
        Every = 1,
        WhenEqual = 2,
        WhenEqualOrGreater = 3
    }

    public class LuaBreakpoint
    {
        private readonly LuaProcess _process;
        private readonly string _filename;
        private readonly int _lineNo, _breakpointId;

        public LuaBreakpoint(
            LuaProcess process,
            string filename,
            int lineNo,
            int breakpointId
        )
        {
            _process = process;
            _filename = filename;
            _lineNo = lineNo;
            _breakpointId = breakpointId;
        }

        /// <summary>
        /// Requests the remote process enable the break point.  An event will be raised on the process
        /// when the break point is received.
        /// </summary>
        public async Task AddAsync(CancellationToken ct)
        {
            await _process.BindBreakpointAsync(this, ct);
        }

        /// <summary>
        /// Removes the provided break point
        /// </summary>
        public Task RemoveAsync(CancellationToken ct)
        {
            return _process.RemoveBreakpointAsync(this, ct);
        }

        public Task DisableAsync(CancellationToken ct)
        {
            return _process.DisableBreakpointAsync(this, ct);
        }

        internal int Id => _breakpointId;

        public string Filename => _filename;

        public int LineNo => _lineNo;

        internal Task SetConditionAsync(LuaBreakpointConditionKind kind, string condition, CancellationToken ct)
        {
            //_conditionKind = kind;
            //_condition = condition;
            return _process.SetBreakpointConditionAsync(this, ct);
        }

        internal Task SetPassCountAsync(LuaBreakpointPassCountKind kind, int passCount, CancellationToken ct)
        {
            //_passCountKind = kind;
            //_passCount = passCount;
            return _process.SetBreakpointPassCountAsync(this, ct);
        }

        internal Task<int> GetHitCountAsync(CancellationToken ct = default(CancellationToken))
        {
            return _process.GetBreakpointHitCountAsync(this, ct);
        }

        internal Task SetHitCountAsync(int count, CancellationToken ct = default(CancellationToken))
        {
            return _process.SetBreakpointHitCountAsync(this, count, ct);
        }
    }
}
