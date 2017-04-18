using Irony.Ast;
using Irony.Parsing;
using Irony.Interpreter.Ast;
using System.Collections.Generic;

namespace NPLTools.IronyParser.Ast{
    public class LuaFuncIdentifierNode : LuaNode
    {
        public List<string> Namespaces { get; set; }
        public string Name { get; set; }
        private static int anonID = 0;
        private string name = "";

        public LuaFuncIdentifierNode()
        {
            Namespaces = new List<string>();
            Name = "";
        }

        internal LuaFuncIdentifierNode InitAnonymous()
        {
            name = "anonfunc" + anonID++;
            AsString = name;
            return this;
        }

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            foreach (var node in treeNode.ChildNodes)
            {
                if (node.Term.Name == "identifier including namespace")
                {
                    if (node.ChildNodes.Count == 0)
                        continue;
                    name += node.ChildNodes[0].Token.Value;
                    for (int i = 1; i < node.ChildNodes.Count; ++i)
                    {
                        Namespaces.Add(node.ChildNodes[i - 1].Token.Value.ToString());
                        name += "." + node.ChildNodes[i].Token.Value;
                    }
                    Name = node.ChildNodes[node.ChildNodes.Count - 1].Token.Value.ToString();
                }

                if (node.Term.Name == "colon call" && node.ChildNodes.Count > 0)
                    name += ":" + node.ChildNodes[1];
            }

            AsString = name;
        }
    }
}