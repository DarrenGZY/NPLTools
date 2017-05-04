﻿//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Xml.Linq;
//using System.Xml.XPath;
//using System.Text.RegularExpressions;


//namespace NPLTools.Intellisense
//{
//    /// <summary>
//    /// Loads declarations and documentation from XML files.
//    /// </summary>
//    public class XmlDocumentationLoader
//    {
//        private readonly List<XElement> docs = new List<XElement>();
//        private HashSet<String> loadedDocsFileName = new HashSet<string>();
//        /// <summary>
//        /// Loads the XML from the given path and adds the declarations to the declaration CodeSense provider.
//        /// </summary>
//        /// <param name="path"></param>
//        public void LoadXml(string path)
//        {
//            if (path == null)
//                throw new ArgumentNullException("path");

//            string filename = System.IO.Path.GetFileName(path);
//            if (!loadedDocsFileName.Contains(filename))
//            {
//                loadedDocsFileName.Add(filename);
//                // Load the XML document
//                XElement doc = ValidateAndLoadXmlDocument(path);

//                // Add the XML document to the list of documents
//                docs.Add(doc);
//            }
//        }

//        /// <summary>
//        /// Adds the declarations from the documentation to a TableDeclarationProvider.
//        /// </summary>
//        /// <param name="tableDeclarationProvider"></param>
//        public void AddDeclarations()
//        {
//            foreach (XElement doc in docs)
//            {
//                // Query the documentation for tables and add them
//                doc.XPathSelectElements("./tables/table").ForEach(table => this.AddTableDeclaration(doc, table));

//                if (doc.Element("globals") != null)
//                {
//                    // Query the documentation for global declarations and add them to the global table
//                    doc.Element("globals").Elements().ForEach(element => AddDeclaration(TableDeclarationProvider.DeclarationsTable, element));
//                }
//            }
//        }

//        private void AddTableDeclaration(XElement doc, XElement table)
//        {
//            //Add a declaration to the global list
//            var name = (string)table.Attribute("name");
//            string src = (string)table.Attribute("src");
//            XElement variableElement = doc.XPathSelectElement(String.Format("./variables/variable[@name='{0}']", name));

//            // Check whether the table inherits declarations from another table
//            XAttribute inherits = table.Attribute("inherits");
//            if (inherits != null)
//            {
//                // inherits is a comma-delimited list of tables that this table inherits from
//                string[] inheritValues = inherits.Value.Split(',');

//                foreach (string inheritValue in inheritValues)
//                {
//                    // Query the table that the declarations should be inherited from
//                    XElement baseTable = doc.XPathSelectElement(String.Format("./tables/table[@name='{0}']", inheritValue.Trim()));

//                    // If the table was found, add each declaration from the base table
//                    if (baseTable != null)
//                        baseTable.Elements().ForEach(element => AddDeclaration(name, element));
//                }
//            }

//            // Go through all declarations and add them to the table
//            table.Elements().ForEach(element => AddDeclaration(name, element, src));
//        }

//        /// <summary>
//        /// Adds the declaration.
//        /// </summary>
//        /// <param name="tableDeclarationProvider">The table declaration provider.</param>
//        /// <param name="tableName">Name of the table.</param>
//        /// <param name="element">The element.</param>
//        private static void AddDeclaration(string tableName, XElement element, string src = null)
//        {
//            try
//            {
//                Declaration declaration = XmlDocumentationLoader.CreateDeclaration(element, src);

//                // Add the declaration 
//                if (declaration != null)
//                    tableDeclarationProvider.AddFieldDeclaration(tableName, declaration);
//            }
//            catch (ArgumentException)
//            {
//                // Enum.Parse could not parse the declaration type, there's nothing we can do.
//            }
//        }

//        /// <summary>
//        /// Creates the declaration.
//        /// </summary>
//        /// <param name="element">The element.</param>
//        /// <returns></returns>
//        private static Declaration CreateDeclaration(XElement element, string src)
//        {
//            try
//            {
//                // Get the summary of the declaration, if available
//                string summary = element.Element("summary") != null ? element.Element("summary").Value.Trim() : null;

//                // Get the type of the declared variable or field, if available
//                string type = element.Attribute("type") != null ? element.Attribute("type").Value : null;

//                var declarationType = (DeclarationType)Enum.Parse(typeof(DeclarationType), element.Name.LocalName, true);

//                if (declarationType == DeclarationType.Function)
//                {
//                    Parameter[] parameters = element.Elements("parameter").Select(parameter => new Parameter
//                    {
//                        DeclarationType = DeclarationType.Parameter,
//                        Name = parameter.Attribute("name").Value,
//                        Type = parameter.Element("type") != null ? parameter.Element("type").Value.Trim() : null,
//                        Description = String.Format("{0}-{1}", parameter.Attribute("name").Value, parameter.Value),
//                        Optional = parameter.Attribute("optional") != null ? Boolean.Parse(parameter.Attribute("optional").Value) : false
//                    })
//                                                                      .ToArray();
//                    return new Method
//                    {
//                        Name = (string)element.Attribute("name"),
//                        DeclarationType = declarationType,
//                        Description = summary,
//                        Type = type,
//                        Parameters = parameters,
//                        FilenameDefinedIn = element.Attribute("src") == null ? src : (string)element.Attribute("src"),
//                        LineDefined = (string)element.Attribute("line"),
//                    };
//                }

//                // Create a declaration for the element
//                return new Declaration
//                {
//                    Name = (string)element.Attribute("name"),
//                    DeclarationType = declarationType,
//                    Description = summary,
//                    Type = type
//                };
//            }
//            catch (ArgumentException)
//            {
//                // Enum.Parse could not parse the declaration type, there's nothing we can do.
//                return null;
//            }
//        }

//        /// <summary>
//        /// Validates the and load XML document.
//        /// </summary>
//        /// <param name="path">The path.</param>
//        /// <returns></returns>
//        private static XElement ValidateAndLoadXmlDocument(string path)
//        {
//            var validator = new XmlValidator();
//            using (StreamReader sr = File.OpenText(path))
//            {
//                string xmlContent = sr.ReadToEnd();
//                using (Stream schemaStream = typeof(XmlDocumentationLoader).Assembly.GetManifestResourceStream(
//                    "NPLProject.Resources.LuaDoc.xsd"))
//                {
//                    if (validator.Validate(xmlContent, schemaStream))
//                        return XElement.Load(path);

//                    throw new ApplicationException(validator.ErrorMessage);
//                }
//            }
//        }
//    }

//    public static class Extenssion
//    {
//        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
//        {
//            if (collection != null)
//            {
//                foreach (var item in collection)
//                {
//                    action(item);
//                }
//            }
//        }
//    }
//}
