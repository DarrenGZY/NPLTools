using System;
using System.Collections.Generic;
using System.Text;

namespace NPLTools.Debugger.DebugEngine
{
    // These are well-known guids in AD7. Most of these are used to specify filters in what the debugger UI is requesting.
    // For instance, guidFilterLocals can be passed to IDebugStackFrame2::EnumProperties to specify only locals are requested.
    static class AD7Guids
    {
        static private Guid _guidFilterRegisters = new Guid("A311DC6A-7280-41D2-9162-7350335C0225");
        static public Guid guidFilterRegisters
        {
            get { return _guidFilterRegisters; }
        }

        static private Guid _guidFilterLocals = new Guid("68552373-571C-4F0A-95CE-9FDA5D51451F");
        static public Guid guidFilterLocals
        {
            get { return _guidFilterLocals; }
        }

        static private Guid _guidFilterAllLocals = new Guid("6110ECB3-8BB7-46E9-B781-32E9F09A3445");
        static public Guid guidFilterAllLocals
        {
            get { return _guidFilterAllLocals; }
        }

        static private Guid _guidFilterArgs = new Guid("04A24526-E521-4342-8E48-0528F361978A");
        static public Guid guidFilterArgs
        {
            get { return _guidFilterArgs; }
        }

        static private Guid _guidFilterLocalsPlusArgs = new Guid("85ED3F51-0889-41B9-9E1D-4D55AD516C7A");
        static public Guid guidFilterLocalsPlusArgs
        {
            get { return _guidFilterLocalsPlusArgs; }
        }

        static private Guid _guidFilterAllLocalsPlusArgs = new Guid("85ED3F51-0889-41B9-9E1D-4D55AD516C7A");
        static public Guid guidFilterAllLocalsPlusArgs
        {
            get { return _guidFilterAllLocalsPlusArgs; }
        }

        // Language guid for Lua. Used when the language for a document context or a stack frame is requested.
        static private Guid _guidLanguageLua = new Guid("F2621D2D-4D68-4BB2-80F4-FEC6F8B6DFDC");
        static public Guid guidLanguageLua
        {
            get { return _guidLanguageLua; }
        }
    }
}
