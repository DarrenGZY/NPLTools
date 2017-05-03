using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLTools.IronyParser.Ast
{
    public class Declaration
    {
        public string Name;
        public ScopeSpan Scope;
        public string FilePath;
        public Declaration NameSpace;
        public HashSet<Declaration> Siblings = new HashSet<Declaration>();

        public Declaration(string name, string filepath)
        {
            Name = name;
            FilePath = filepath;
            Scope = new ScopeSpan();
            NameSpace = null;
        }

        public Declaration(string name, string filepath, Declaration Namespace)
        {
            Name = name;
            FilePath = filepath;
            Scope = new ScopeSpan();
            NameSpace = Namespace;
        }

        public Declaration(string name, string filepath, ScopeSpan scope)
        {
            Name = name;
            FilePath = filepath;
            Scope = scope;
            NameSpace = null;
        }

        public Declaration(string name, string filepath, ScopeSpan scope, Declaration Namespace)
        {
            Name = name;
            FilePath = filepath;
            Scope = scope;
            NameSpace = Namespace;
        }

        public bool Equal(Declaration opponent)
        {
            if (opponent == null)
                return false;
            if (Name == opponent.Name)
            {
                if (NameSpace == null && opponent.NameSpace == null)
                {
                    return true;
                }
                else if (NameSpace != null)
                {
                    if (NameSpace.Equal(opponent.NameSpace))
                        return true;
                    foreach (var sibling in NameSpace.Siblings)
                        if (sibling.Equal(opponent.NameSpace))
                            return true;
                }
            }

            foreach (var sibling in Siblings)
                if (sibling.Equal(opponent))
                    return true;

            return false;
        }

        public bool Equal(string name)
        {
            return Equal(BuildDummyDeclaration(name));
        }

        private Declaration BuildDummyDeclaration(string name)
        {
            int index = name.LastIndexOf('.');
            if (index == -1)
                return new Declaration(name, "");
            else
            {
                //string a = name.Substring(index+1);
                //string b = name.Substring(0, index);
                return new Declaration(name.Substring(index + 1), "", BuildDummyDeclaration(name.Substring(0, index)));
            }
        }

        //public bool NameEqual(string name)
        //{
        //    int index = name.LastIndexOf('.');
        //    if (index == -1)
        //        return this.Equal(new Declaration(name));
        //    else
        //        return this.Equal(new Declaration(name.Substring(0, index), new Declaration(name.Substring(index + 1))));
        //}

        //public void AddField(Declaration field)
        //{
        //    Fields.Add(field);
        //}

        public void AddSibling(Declaration sibling)
        {
            Siblings.Add(sibling);
        }
    }
}
