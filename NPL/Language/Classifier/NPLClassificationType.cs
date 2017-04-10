using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace NPLTools.Language.Classifier
{
    internal static class NPLClassificationDefinition
    {

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("Keyword")]
        internal static ClassificationTypeDefinition NPLKeyword = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("Id")]
        internal static ClassificationTypeDefinition NPLIdentifier = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("String")]
        internal static ClassificationTypeDefinition NPLString = null;
        
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("Comment")]
        internal static ClassificationTypeDefinition NPLComment = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("Text")]
        internal static ClassificationTypeDefinition NPLText = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("Number")]
        internal static ClassificationTypeDefinition NPLNumber = null;

    }
}
