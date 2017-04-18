using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Irony.Parsing;
using Irony;
using NPLTools.Tests;
//using NPLTools.Grammar;
//using NPLTools.Grammar.Ast;
using NPLTools.IronyParser;
using Irony.Interpreter.Ast;

namespace NPL.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            //LuaGrammar grammar = new LuaGrammar();
            Irony.Parsing.Parser parser = new Irony.Parsing.Parser(new NPLTools.IronyParser.LuaGrammar());

            string code = "local a = {}  function a.b() end";
            Irony.Parsing.ParseTree tree = parser.Parse(code);
            
            PrintTree(tree);
            PrintAstTree(tree);
            //tree.
            LogMessageList m = tree.ParserMessages;
            if (m[0] != null)
                Console.WriteLine(m[0].Message);
        }

        private void PrintTree(ParseTree tree)
        {
            ParseTreeNode root = tree.Root;
            PrintNode(root, 0);
        }

        private void PrintAstTree(ParseTree tree)
        {
            AstNode root = tree.Root.AstNode as AstNode;
            PrintAstNode(root, 0);
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

        private void PrintAstNode(AstNode node, int indent)
        {
            string indents = "";
            for (int i = 0; i < indent; ++i)
                indents += "    ";
            Debug.WriteLine(indents + node + "(" + node.Span.Location.Position + " - " + (node.Span.Location.Position + node.Span.Length) + ")");
            foreach (AstNode child in node.ChildNodes)
            {
                PrintAstNode(child, indent + 1);
            }
        }
    }
}
