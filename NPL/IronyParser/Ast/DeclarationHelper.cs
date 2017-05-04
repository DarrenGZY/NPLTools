using NPLTools.Intellisense;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLTools.IronyParser.Ast
{
    public static class DeclarationHelper
    {
        public static bool TryGetExpressionDeclaration(LuaNode expr, LuaBlockNode block, LuaModel model, out Declaration declaration)
        {
            declaration = null;
            if (expr is LuaIdentifierNode)
            {
                foreach (var localDeclaration in block.Locals)
                {
                    if (expr.AsString == localDeclaration.Name)
                    {
                        declaration = localDeclaration;
                        return true;
                    }
                }
                foreach (var globalDeclaration in block.Globals)
                {
                    if (expr.AsString == globalDeclaration.Name)
                    {
                        declaration = globalDeclaration;
                        return true;
                    }
                }
                foreach (var globalDeclaration in model.GetGlobalDeclarationInProject())
                {
                    if (expr.AsString == globalDeclaration.Name)
                    {
                        declaration = globalDeclaration;
                        // clear siblings in case of adding duplicate declaration
                        declaration.ClearSiblingsinFile(model.FilePath);
                        return true;
                    }
                }
            }
            else if (expr is LuaTableAccessNode)
            {
                foreach (var localDeclaration in block.Locals)
                {
                    Declaration dummyDeclaration = BuildDeclaration(expr.AsString);
                    if (dummyDeclaration.Equal(localDeclaration))
                    {
                        declaration = localDeclaration;
                        return true;
                    }
                }
                foreach (var globalDeclaration in block.Globals)
                {
                    Declaration dummyDeclaration = BuildDeclaration(expr.AsString);
                    if (dummyDeclaration.Equal(globalDeclaration))
                    {
                        declaration = globalDeclaration;
                        return true;
                    }
                }
                foreach (var globalDeclaration in model.GetGlobalDeclarationInProject())
                {
                    Declaration dummyDeclaration = BuildDeclaration(expr.AsString);
                    if (dummyDeclaration.Equal(globalDeclaration))
                    {
                        declaration = globalDeclaration;
                        // clear siblings in case of adding duplicate declaration
                        declaration.ClearSiblingsinFile(model.FilePath);
                        return true;
                    }
                }
            }
            return false;
        }

        public static Declaration BuildDeclaration(string name)
        {
            int index = name.LastIndexOf('.');
            if (index == -1)
                return new Declaration(name, "");
            else
            {
                //string a = name.Substring(index+1);
                //string b = name.Substring(0, index);
                return new Declaration(name.Substring(index + 1), "", BuildDeclaration(name.Substring(0, index)));
            }
        }
    }
}
