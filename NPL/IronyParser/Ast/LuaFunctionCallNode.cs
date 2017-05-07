using System;
using Irony.Ast;
using Irony.Parsing;
using NPLTools.Intellisense;
using NPLTools.IronyParser;
using System.IO;
using System.Threading.Tasks;

namespace NPLTools.IronyParser.Ast
{
    public class LuaFunctionCallNode : LuaNode, IDeclaration
    {
        public LuaNode Target;
        public LuaNode Arguments;
        public LuaNode Name;

        public LuaFunctionCallNode()
        {

        }

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            // prefixexp args
            if (treeNode.ChildNodes.Count == 2)
            {
                Target = AddChild(String.Empty, treeNode.ChildNodes[0]) as LuaNode;
                Arguments = AddChild(String.Empty, treeNode.ChildNodes[1]) as LuaNode;
                Name = null;
            }
            // prefixexp `:´ Name args 
            else if (treeNode.ChildNodes.Count == 4)
            {
                Target = AddChild(String.Empty, treeNode.ChildNodes[0]) as LuaNode;
                Name = AddChild(String.Empty, treeNode.ChildNodes[2]) as LuaNode;
                Arguments = AddChild(String.Empty, treeNode.ChildNodes[3]) as LuaNode;
            }
            
            //foreach (var node in treeNode.ChildNodes)
            //{
            //    if (node.Term.Name == "identifier")
            //        name += node.FindTokenAndGetText();

            //    if (node.Term.Flags.IsSet(TermFlags.IsOperator))
            //        name += node.Term.Name;

            //    if (node.Term.Name == "expr list")
            //        Arguments = AddChild("Args", node);
            //}
            
            AsString = "Function Call";
        }

        public void GetDeclarations(LuaBlockNode block, LuaModel model)
        {
            // require("sample.lua")
            if (Target != null &&
                Target.AsString == "require" && 
                Arguments.ChildNodes.Count == 1 &&
                Arguments.ChildNodes[0] is LuaLiteralNode &&
                ((LuaLiteralNode)Arguments.ChildNodes[0]).Type == LuaType.String)
            {
                string fileName = ((LuaLiteralNode)Arguments.ChildNodes[0]).Value;
                fileName = fileName.Substring(1, fileName.Length - 2);
                try
                {
                    string filePath = Path.Combine(Path.GetDirectoryName(model.FilePath), fileName);
                    // project mode
                    if (model.Entry != null && model.Entry.Analyzer != null && model.Entry.Analyzer.ContainsFile(filePath))
                    {
                        AnalysisEntry requiredEntry = model.Entry.Analyzer.GetAnalysisEntry(filePath);
                        if (requiredEntry.Model != null)
                            block.Requires.AddRange(requiredEntry.Model.GetGlobalDeclarations());
                        model.AddIncludedFile(filePath, requiredEntry.Model);
                    }
                    // singleton mode
                    else
                    {
                        string source = File.ReadAllText(filePath);
                        Irony.Parsing.Parser parser = new Irony.Parsing.Parser(LuaGrammar.Instance);
                        ParseTree tree = parser.Parse(source);
                        LuaModel requiredModel = new LuaModel(tree, filePath);
                        block.Requires.AddRange(requiredModel.GetGlobalDeclarations());
                        model.AddIncludedFile(filePath, requiredModel);
                    }
                }
                catch (Exception e)
                {

                }
            }
        }
    }//class
}
