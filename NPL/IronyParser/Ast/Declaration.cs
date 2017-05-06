using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLTools.IronyParser.Ast
{
    public class Declaration
    {
        /// <summary>
        /// The name of declaration 
        /// @example: local foo; 
        /// "foo" is the name
        /// </summary>
        public string Name;

        /// <summary>
        /// The scope span of declaration
        /// </summary>
        public ScopeSpan Scope;

        /// <summary>
        /// The file path
        /// </summary>
        public string FilePath;

        /// <summary>
        /// The namespace of declaration
        /// </summary>
        public Declaration NameSpace;

        /// <summary>
        /// The equals of declaration
        /// @example: local a = b
        /// here "a" is a sibling of "b"
        /// </summary>
        public HashSet<Declaration> Siblings = new HashSet<Declaration>();

        /// <summary>
        /// The comment description of the declaration
        /// </summary>
        public string Description;

        // For dummy declaration construct
        public Declaration(string name)
        {
            Name = name;
            Description = "";
            FilePath = "";
            Scope = new ScopeSpan();
            NameSpace = null;
        }

        // For dummy declaration construct
        public Declaration(string name, Declaration nameSpace)
        {
            Name = name;
            Description = "";
            FilePath = "";
            Scope = new ScopeSpan();
            NameSpace = nameSpace;
        }

        public Declaration(string name, string description, string filepath)
        {
            Name = name;
            Description = description;
            FilePath = filepath;
            Scope = new ScopeSpan();
            NameSpace = null;
        }

        //public Declaration(string name, string filepath, Declaration nameSpace)
        //{
        //    Name = name;
        //    FilePath = filepath;
        //    Scope = new ScopeSpan();
        //    NameSpace = nameSpace;
        //}

        public Declaration(string name, string description, string filepath, ScopeSpan scope)
        {
            Name = name;
            Description = description;
            FilePath = filepath;
            Scope = scope;
            NameSpace = null;
        }

        public Declaration(string name, string description, string filepath, ScopeSpan scope, Declaration nameSpace)
        {
            Name = name;
            Description = description;
            FilePath = filepath;
            Scope = scope;
            NameSpace = nameSpace;
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
                    //foreach (var sibling in NameSpace.Siblings)
                    //    if (sibling.Equal(opponent.NameSpace))
                    //        return true;
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
                return new Declaration(name);
            else
            {
                //string a = name.Substring(index+1);
                //string b = name.Substring(0, index);
                return new Declaration(name.Substring(index + 1), BuildDummyDeclaration(name.Substring(0, index)));
            }
        }

        public void ClearSiblingsinFile(string path)
        {
            Siblings.RemoveWhere((sibling) => sibling.FilePath == path);
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
