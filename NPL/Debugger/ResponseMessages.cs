using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NPLTools.Debugger
{
    public class ResponseMessages
    {
        public void Dispatch(string response)
        {
            string[] pieces = response.Split(' ');
            if (pieces.Length > 0)
            {
                int code; 
                if (Int32.TryParse(pieces[0], out code))
                {
                    switch (code)
                    {
                        case BreakpointHitMsg.Code:
                            if (pieces.Length != 2)
                                return;

                            break;

                    }
                }
                
            }
        }
    }

    //public interface BasicMsg
    //{
    //    static int Code { get; }
    //    string Name { get; }
    //}

    public class BreakpointHitMsg 
    {
        //private int _code = 0x01;
        //private readonly string _name = "BREAKPOINTHIT";
        private int _id;

        public BreakpointHitMsg(int id)
        {
            _id = id;
        }

        public const int Code = 201;
        public string Name = "BREAKPOINTHIT";
        public int Id => _id;
    }
}
