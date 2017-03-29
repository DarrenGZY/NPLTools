using Irony.Ast;
using Irony.Parsing;
using Irony.Interpreter.Ast;

namespace NPLTools.IronyParser.Ast{
    public class LuaFuncIdentifierNode : AstNode {
        private static int anonID = 0;
        private string name = "";

        internal LuaFuncIdentifierNode InitAnonymous() {
            name = "anonfunc" + anonID++;
            AsString = name;
            return this;
        }

        public override void Init(AstContext context, ParseTreeNode treeNode) {
            base.Init(context, treeNode);

            foreach (var node in treeNode.ChildNodes)
            {
                if (node.Term.Name == "identifier including namespace")
                    name += node.ChildNodes[0].Token.ValueString;

                if (node.Term.Name == "colon call" && node.ChildNodes.Count > 0)
                    name += ":" + node.ChildNodes[1];
            }

            AsString = name;
        }
    }
}