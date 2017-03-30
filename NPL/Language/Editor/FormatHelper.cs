using Irony.Interpreter.Ast;
using Irony.Parsing;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using NPLTools.IronyParser;
using NPLTools.IronyParser.Ast;
//using NPLTools.Grammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLTools.Language.Editor
{
    internal static class FormatHelper
    {
        public static bool FormatBlock(ITextView view)
        {
            int startLine = view.Selection.Start.Position.GetContainingLine().LineNumber;
            int endLine = view.Selection.End.Position.GetContainingLine().LineNumber;

            ITextSnapshot snapshot = view.TextSnapshot;

            int[] indentations;
            RetrieveIndentationsFromSyntaxTree(view, out indentations);
            //AstNode root = NPLTextViewCreationListener.AstRoot;
            Parser parser = new Parser(LuaGrammar.Instance);

            //ParseTree tree = parser.Parse(view.TextSnapshot.GetText());
            //rule 1: insert space before and after binary operator if there not any
            //rule 2: insert space after comma, semicolon if there not any
            //rule 3: indentation increase inside block
            //rule 4: multiple spaces replaced by a single space
            Scanner scanner = parser.Scanner;
            using (var edit = view.TextBuffer.CreateEdit())
            {
                //IEnumerable<ITextSnapshotLine> lines = view.TextSnapshot.Lines;
                for (int lineNumber = startLine; lineNumber <= endLine; lineNumber++)
                {
                    ITextSnapshotLine line = snapshot.GetLineFromLineNumber(lineNumber);
                    int lineOffset = line.Start.Position;
                    string lineText = line.GetText();

                    scanner.VsSetSource(lineText, 0);

                    int state = 0;
                    Token currentToken = scanner.VsReadToken(ref state);
                    Token lastToken = null;
                    // add space before the first token
                    if (currentToken != null)
                    {
                        Span editSpan = new Span(lineOffset, currentToken.Location.Position);
                        string indentation = "";
                        for (int i = 0; i < indentations[lineNumber]; ++i)
                            indentation += "\t";
                        edit.Replace(editSpan, indentation);
                    }

                    while (currentToken != null)
                    {
                        Token nextToken = scanner.VsReadToken(ref state);
                        if (currentToken.Text == "+" ||
                            currentToken.Text == "=" ||
                            currentToken.Text == "*" ||
                            currentToken.Text == "\\" ||
                            currentToken.Text == "-")
                        {
                            if (lastToken != null)
                            {
                                int spaceStart = lastToken.Location.Position + lastToken.Length;
                                int spaceLength = currentToken.Location.Position - spaceStart;
                                if (spaceLength != 1)
                                {
                                    Span span = new Span(lineOffset + spaceStart, spaceLength);
                                    edit.Replace(span, " ");
                                }
                            }

                            if (nextToken != null)
                            {
                                int spaceStart = currentToken.Location.Position + currentToken.Length;
                                int spaceLength = nextToken.Location.Position - spaceStart;
                                if (spaceLength != 1)
                                {
                                    Span span = new Span(lineOffset + spaceStart, spaceLength);
                                    edit.Replace(span, " ");
                                }
                            }
                        }
                        else if (currentToken.Text == "," ||
                            currentToken.Text == ";")
                        {
                            if (nextToken != null)
                            {
                                int spaceStart = currentToken.Location.Position + currentToken.Length;
                                int spaceLength = nextToken.Location.Position - spaceStart;
                                if (spaceLength != 1)
                                {
                                    Span span = new Span(lineOffset + spaceStart, spaceLength);
                                    edit.Replace(span, " ");
                                }
                            }
                        }

                        lastToken = currentToken;
                        currentToken = nextToken;
                    }
                }
                edit.Apply();
            }
            
            
            PrintAstTree(NPLTextViewCreationListener.ParseTree);
            return false;
        }

        private static void RetrieveIndentationsFromSyntaxTree(ITextView view, out int[] indentations)
        {
            int lineNumber = view.TextSnapshot.LineCount;
            indentations = new int[lineNumber];
            LuaNode root = NPLTextViewCreationListener.AstRoot;
            for (int i = 0; i < lineNumber; ++i)
            {
                indentations[i] = -1;
            }
            IterateAstTree(view, root, indentations);
        }

        private static void IterateAstTree(ITextView view, LuaNode node, int[] indentations)
        {
            if (node is LuaBlockNode)
            {
                for (int i = node.Span.Location.Line; 
                    i <= view.TextSnapshot.GetLineNumberFromPosition(node.Span.EndPosition - 1); ++i)
                {
                    indentations[i] += 1;
                }
            }
            foreach (LuaNode childNode in node.GetChildNodes())
            {
                IterateAstTree(view, childNode, indentations);
            }
        }

        private static void PrintAstTree(ParseTree tree)
        {
            AstNode root = tree.Root.AstNode as AstNode;
            PrintAstNode(root, 0);
        }

        private static void PrintAstNode(AstNode node, int indent)
        {
            string indents = "";
            for (int i = 0; i < indent; ++i)
                indents += "    ";
            System.Diagnostics.Debug.WriteLine(indents + node + "(" + node.Span.Location.Position + " - " + (node.Span.Location.Position + node.Span.Length) + ")");
            foreach (AstNode child in node.ChildNodes)
            {
                PrintAstNode(child, indent + 1);
            }
        }
    }
}
