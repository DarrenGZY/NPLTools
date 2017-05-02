using Irony.Interpreter.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Ast;
using Irony.Parsing;

namespace NPLTools.IronyParser.Ast
{
    public class LuaBlockNode : LuaNode
    {
        public AstNodeList Statements
        {
            get { return ChildNodes; }
        }

        public List<Declaration> Locals;
        public List<Declaration> Globals;
        public List<RequiredDeclaration> Requires;

        public Scope Scope;
        public LuaBlockNode()
        {
            Locals = new List<Declaration>();
            Globals = new List<Declaration>();
            Requires = new List<RequiredDeclaration>();
        }

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            Scope = new Scope(treeNode.Span.Location.Position, treeNode.Span.EndPosition);

            ParseTreeNode statements = treeNode.ChildNodes.Count > 0 ? treeNode.ChildNodes[0] : null;
            if (statements == null) return;

            foreach (ParseTreeNode child in statements.ChildNodes)
            {
                AddChild(String.Empty, child);

                if (child.AstNode is IDeclaration)
                {
                    ((IDeclaration)child.AstNode).GetDeclarations(this);
                }
            }

            AsString = "Block";
        }
    }

    public struct Scope
    {
        public int startPosition;
        public int endPosition;
        public Scope(int startPosition, int endPosition)
        {
            this.startPosition = startPosition;
            this.endPosition = endPosition;
        }
    }

    public struct ScopeSpan
    {
        public int StartPosition;
        public int StartLine;
        public int EndPosition;
        public int EndLine;
        public ScopeSpan(int startPosition, int startLine, int endPosition, int endLine)
        {
            StartPosition = startPosition;
            StartLine = startLine;
            EndPosition = endPosition;
            EndLine = endLine;
        }
    }

    public class Declaration
    {
        public string Name;
        public ScopeSpan Scope;
        public Declaration NameSpace;

        public Declaration(string name)
        {
            int index = name.LastIndexOf('.');
            if (index == -1)
            {
                Name = name;
                Scope = new ScopeSpan();
                NameSpace = null;
            }
            else
            {
                Name = name.Substring(index + 1);
                Scope = new ScopeSpan();
                NameSpace = new Declaration(name.Substring(0, index));
            }
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
                if ((NameSpace == null && opponent.NameSpace == null) ||
                    (NameSpace != null && NameSpace.Equal(opponent.NameSpace)))
                    return true;
            }
            return false;
        }

        public bool NameEqual(string name)
        {
            int index = name.LastIndexOf('.');
            if (index == -1)
                return this.Equal(new Declaration(name));
            else
                return this.Equal(new Declaration(name.Substring(0, index), new Declaration(name.Substring(index+1))));
        }
    }

    public class RequiredDeclaration
    {
        public string FilePath;
        public ScopeSpan Scope;
        public RequiredDeclaration(string filePath, ScopeSpan scope)
        {
            FilePath = filePath;
            Scope = scope;
        }
    }
}
