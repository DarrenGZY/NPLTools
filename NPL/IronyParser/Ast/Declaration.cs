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
        public Declaration NameSpace;
        //public HashSet<Declaration> Fields = new HashSet<Declaration>();
        public HashSet<Declaration> Siblings = new HashSet<Declaration>();

        public Declaration(string name)
        {
            Name = name;
            Scope = new ScopeSpan();
            NameSpace = null;
        }

        public Declaration(string name, Declaration Namespace)
        {
            Name = name;
            Scope = new ScopeSpan();
            NameSpace = Namespace;
        }

        public Declaration(string name, ScopeSpan scope)
        {
            Name = name;
            Scope = scope;
            NameSpace = null;
        }

        public Declaration(string name, ScopeSpan scope, Declaration Namespace)
        {
            Name = name;
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

        public bool NameEqual(string name)
        {
            int index = name.LastIndexOf('.');
            if (index == -1)
                return this.Equal(new Declaration(name));
            else
                return this.Equal(new Declaration(name.Substring(0, index), new Declaration(name.Substring(index + 1))));
        }

        public void AddField(Declaration field)
        {
            Fields.Add(field);
        }

        public void AddSibling(Declaration sibling)
        {
            Siblings.Add(sibling);
        }
    }
}
