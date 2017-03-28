#region License
/* **********************************************************************************
 * Copyright (c) Roman Ivantsov
 * This source code is subject to terms and conditions of the MIT License
 * for Irony. A copy of the license can be found in the License.txt file
 * at the root of this distribution. 
 * By using this source code in any fashion, you are agreeing to be bound by the terms of the 
 * MIT License.
 * You must not remove this notice from this software.
 * **********************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Irony.Ast;
using Irony.Interpreter.Ast;
using Irony.Interpreter;
using Irony.Parsing;

namespace NPLTools.IronyParser.Ast
{

    //A node representing function definition
    public class LuaFunctionDefNode : AstNode//, ICallTarget
    {
        AstNode NameNode;
        AstNode Parameters;
        AstNode Body;

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            //child #0 is usually a keyword like "local" or "local function"
            int i = treeNode.ChildNodes.Count-1;

            Debug.Assert(i>1);
            var anon = treeNode.ChildNodes[i - 2];
            
            if (anon != null && anon.Token != null && anon.Token.KeyTerm != null)
            {
                NameNode = new LuaFuncIdentifierNode().InitAnonymous();; 
                
            }
            else
            {
                NameNode = AddChild("Name", anon);
            }
            var name = NameNode.AsString;

            Parameters = AddChild("Parameters", treeNode.ChildNodes[i-1]);
            Body = AddChild("Body", treeNode.ChildNodes[i]);
            
            AsString = "<Function " + name + ">";
        }

        //public override void EvaluateNode(EvaluationContext context, AstMode mode)
        //{
        //    //push the function into the stack
        //    context.Data.Push(this);
        //    NameNode.Evaluate(context, AstMode.Write);
        //}


        //#region ICallTarget Members

        //public void Call(EvaluationContext context)
        //{
        //    context.PushFrame(this.NameNode.ToString(), this, context.CurrentFrame);
        //    Parameters.Evaluate(context, AstMode.None);
        //    Body.Evaluate(context, AstMode.None);
        //    context.PopFrame();
        //}

        //#endregion
    }//class

}//namespace
