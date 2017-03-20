using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Irony.Parsing;
using NPL.Parser;
namespace NPL.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            SampleGrammar grammar = new SampleGrammar();
            Irony.Parsing.Parser parser = new Irony.Parsing.Parser(grammar);

            string code = @"1+2*3@";
            parser.Scanner.VsSetSource(code, 0);

            int state = 0;
            Token token = parser.Scanner.VsReadToken(ref state);
            Token token1 = parser.Scanner.VsReadToken(ref state);
            Token token2 = parser.Scanner.VsReadToken(ref state);
            Token token3 = parser.Scanner.VsReadToken(ref state);
            Token token4 = parser.Scanner.VsReadToken(ref state);
            Token token5 = parser.Scanner.VsReadToken(ref state);

        }
    }
}
