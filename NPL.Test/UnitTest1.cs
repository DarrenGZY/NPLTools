using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Irony.Parsing;
using Irony;
using NPL.Parser;
namespace NPL.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            //LuaGrammar grammar = new LuaGrammar();
            Irony.Parsing.Parser parser = new Irony.Parsing.Parser(LuaGrammar.Instance);

            string code = @"local a = \n local b = ";
            parser.Scanner.VsSetSource(code, 0);
            ParseTree tree = parser.Parse(code);

            LogMessageList m = tree.ParserMessages;
            if (m[0] != null)
                Console.WriteLine(m[0].Message);
        }
    }
}
