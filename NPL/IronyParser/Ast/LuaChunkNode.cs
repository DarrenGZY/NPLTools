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

using Irony.Ast;
using Irony.Interpreter.Ast;
using Irony.Parsing;

namespace NPLTools.IronyParser.Ast
{

    public class LuaChunkNode : AstNode
    {
        public LuaBlockNode Block { get; set; }

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            // Normally Chunk only has one child node, which is a block node
            if (treeNode.ChildNodes.Count > 0)
                Block = AddChild(string.Empty, treeNode.ChildNodes[0]) as LuaBlockNode;

            AsString = "LuaChunk";
        }

    }//class

}

//namespace

