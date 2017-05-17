using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLTools.Project
{
    static class Guids
    {
        public const string guidNPLProjectPkgString =
            "E2113C2D-E364-41B5-B1FF-71FAD9691D2E";
        public const string guidNPLProjectCmdSetString =
            "DD092ECF-972B-471B-AA9B-20845E1DFE4C";
        public const string NPLProjectFactoryGuidString =
            "AFCF7665-3223-4967-A133-AD0F059C8014";
        public const string ProjectNodeGuid =
            "C0A64257-C203-4F7B-923D-0679CDCA1EA2";
        public const string NPLEditorFactoryGuidString =
            "E2FCEC54-5522-453B-81FC-8FB57FAD5E4D";
        public const string NPLLanguageInfoGuidString =
            "CF2A07F9-34D8-43A4-B54F-BD796850A0B4";
        public static readonly Guid guidNPLProjectCmdSet =
            new Guid(guidNPLProjectCmdSetString);
        public static readonly Guid NPLProjectFactoryGuid =
            new Guid(NPLProjectFactoryGuidString);
    }
}
