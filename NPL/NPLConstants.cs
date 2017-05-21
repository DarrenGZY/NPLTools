using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLTools
{
    public static class NPLConstants
    {
        public const string LuaFileExtension = ".lua";
        public const string NPLFileExtension = ".npl";
        public const string PageFileExtension = ".page";
        public const string LuaExtension = "lua";
        public const string NPLExtension = "Lua/NPL";
        public const string ContentType = "NPL";

        // Parse Tree Nodes
        public const string DoBlock = "do block";
        public const string ForBlock = "for block";
        public const string GenericForBlock = "generic for block";
        public const string WhileBlock = "while block";
        public const string RepeatBlock = "repeat block";
        public const string ConditionBlock = "conditonal block";
        public const string FunctionDeclaration = "function declaration";
        public const string LocalFunctionDeclaration = "local function declaration";
    }
}
