using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NPLTools.Debugger
{
    public class ResponseType
    {
        public const string BreakpointHit = "BreakpointHit";
        public const string ModuleLoad = "ModuleLoad";
        public const string FrameList = "FrameList";
        public const string ThreadCreat = "ThreadCreate";
        public const string DebugExit = "DebugExit";
        public const string StepComplete = "StepComplete";
    }

    public class BreakpointHitEvent 
    {
        //private int _code = 0x01;
        //private readonly string _name = "BREAKPOINTHIT";
        private int _id;
        public BreakpointHitEvent(int id)
        {
            _id = id;
        }

        public const int Code = 201;
        public const string Name = "BreakpointHit";
        public int Id => _id;
    }

    public class ModuleLoadEvent
    {
        //private int _code = 0x01;
        //private readonly string _name = "BREAKPOINTHIT";
        private int _id;
        private string _file;
        private const string _name = "ModuleLoad";
        public ModuleLoadEvent(string file, int id)
        {
            _file = file;
            _id = id;
        }

        public const string Name = "ModuleLoad";
        public int Id => _id;
        public string File => _file;
    }

    public class FrameListEvent
    {
        //private int _code = 0x01;
        //private readonly string _name = "BREAKPOINTHIT";
        private int _id;
        private string _file;
        private const string _name = "FrameList";
        public FrameListEvent()
        {

        }

        public const string Name = "FrameList";
        public int Id => _id;
        public string File => _file;
    }

    public class ThreadCreateEvent
    {
        public const string Name = "ThreadCreate";
    }
}
