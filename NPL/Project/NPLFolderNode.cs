using Microsoft.VisualStudioTools.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLTools.Project
{
    internal class NPLFolderNode : CommonFolderNode
    {
        public NPLFolderNode(NPLProjectNode root, ProjectElement e) : base(root, e)
        {

        }
    }
}
