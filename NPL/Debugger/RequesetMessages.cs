﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLTools.Debugger
{
    public static class RequesetMessages
    {
        private const string linebreak = "\n"; 

        public static string SetBreakPoint(string file, int lineNo)
        {
            return String.Format("{0} {1} {2}{3}", DebugCommands.SetBreakpoint, file, lineNo, linebreak);
        }
    }
}