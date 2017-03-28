using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Irony.Parsing;
using Irony;
using NPLTools.Tests;
using NPLTools.Grammar;
using NPLTools.Grammar.Ast;

namespace NPL.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            //LuaGrammar grammar = new LuaGrammar();
            Irony.Parsing.Parser parser = new Irony.Parsing.Parser(LuaGrammarWithAst.Instance);

            string code = @"local a = 1";
            ParseTree tree = parser.Parse(code);

            Assignment node = tree.Root.AstNode as Assignment;
            Node vars = node.VariableList;
            PrintTree(tree);
            //tree.
            ParseTreeNode root = tree.Root;
            ParseTreeNode child = root.ChildNodes[0];
            ParseTreeNode child2 = child.ChildNodes[0];
            ParseTreeNode child3 = child2.ChildNodes[0];
            ParseTreeNode child4 = child3.ChildNodes[0];
            ParseTreeNode child5 = child4.ChildNodes[0];
            ParseTreeNode child6 = child5.ChildNodes[0];
            LogMessageList m = tree.ParserMessages;
            if (m[0] != null)
                Console.WriteLine(m[0].Message);
        }

        private void PrintTree(ParseTree tree)
        {
            ParseTreeNode root = tree.Root;
            PrintNode(root, 0);
        }

        private void PrintNode(ParseTreeNode node, int indent)
        {
            string indents = "";
            for (int i = 0; i < indent; ++i)
                indents += "    ";
            Debug.WriteLine(indents + node + "(" + node.Span.Location.Position + " - " + (node.Span.Location.Position + node.Span.Length) + ")");
            foreach(ParseTreeNode child in node.ChildNodes)
            {
                PrintNode(child, indent+1);
            }
        }
    }
}
