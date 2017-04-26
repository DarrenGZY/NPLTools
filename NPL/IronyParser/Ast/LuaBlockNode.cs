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
        public List<Declaration> NameSpaces;
        public Declaration(string name, ScopeSpan scope)
        {
            Name = name;
            Scope = scope;
            NameSpaces = new List<Declaration>();
        }

        public Declaration(string name, ScopeSpan scope, List<Declaration> namespaces)
        {
            Name = name;
            Scope = scope;
            NameSpaces = new List<Declaration>(namespaces);
        }

        public bool NamesEqual(List<string> names)
        {
            if (NameSpaces.Count != names.Count - 1)
                return false;

            for(int i = 0; i < NameSpaces.Count; ++i)
            {
                if (NameSpaces[i] == null || NameSpaces[i].Name != names[i]) //TODO: why NameSpaces[i] may be null
                    return false;
            }

            if (Name != names[names.Count - 1])
                return false;
            return true;
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
