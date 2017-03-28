using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Ast;
using Irony.Interpreter.Ast;
using Irony.Parsing;

namespace NPLTools.IronyParser.Ast
{
    public class LuaFunctionCallNode : AstNode
    {
        AstNode TargetRef = new AstNode();
        AstNode Arguments;
        string _targetName;

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            string name = "";
            foreach (var node in treeNode.ChildNodes)
            {
                if (node.Term.Name == "identifier")
                    name += node.FindTokenAndGetText();

                if (node.Term.Flags.IsSet(TermFlags.IsOperator))
                    name += node.Term.Name;

                if (node.Term.Name == "expr list")
                    Arguments = AddChild("Args", node);
            }

            _targetName = name;
            
            AsString = "Call " + _targetName;
        }

 //       public override void EvaluateNode(EvaluationContext context, AstMode mode)
 //       {
 //           TargetRef.Evaluate(context, AstMode.Read);
 //           var target = context.Data.Pop() as ICallTarget;
 ////           if (target == null)
 //  //             context.ThrowError(Resources.ErrVarIsNotCallable, _targetName);
 //           Arguments.Evaluate(context, AstMode.Read);
 //           target.Call(context);
 //       }

    }//class
}
