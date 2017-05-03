using NPLTools.Intelligense;
using NPLTools.IronyParser.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLTools.IronyParser.Ast
{
    public interface IDeclaration
    {
        void GetDeclarations(LuaBlockNode block, LuaModel model);
    }
}
