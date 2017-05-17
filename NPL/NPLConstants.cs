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
        public const string NPLExtension = "NPL";
        public const string LanguageName = "NPL";

        // project constants
        public const string NPLExePath = "NPLExePath";
        public const string NPLOptions = "NPLOptions";
        public const string StartupFile = "StartupFile";
        public const string Arguments = "Arguments";
        public const string WorkingDirectory = "WorkingDirectory";

        public const string NPLProjectName = "NPLProject";

        public const string ProjectFileFilter = "NPL Project File (*.nplproj)\n*.nplproj\nAll Files (*.*)\n*.*\n";

        // Parse Tree Nodes
        public const string DoBlock = "do block";
        public const string ForBlock = "for block";
        public const string GenericForBlock = "generic for block";
        public const string WhileBlock = "while block";
        public const string RepeatBlock = "repeat block";
        public const string ConditionBlock = "conditonal block";
        public const string FunctionDeclaration = "function declaration";
        public const string LocalFunctionDeclaration = "local function declaration";

        // Debug Engine
        public const string DebugEngineName = "NPL";
    }
}
