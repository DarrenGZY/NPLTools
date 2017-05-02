using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace NPLTools.Language.Classifier
{
    internal static class NPLClassificationDefinition
    {

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("NPLKeyword")]
        internal static ClassificationTypeDefinition NPLKeyword = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("NPLIdentifier")]
        internal static ClassificationTypeDefinition NPLIdentifier = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("NPLString")]
        internal static ClassificationTypeDefinition NPLString = null;
        
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("NPLComment")]
        internal static ClassificationTypeDefinition NPLComment = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("NPLText")]
        internal static ClassificationTypeDefinition NPLText = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("NPLNumber")]
        internal static ClassificationTypeDefinition NPLNumber = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("NPLSelf")]
        internal static ClassificationTypeDefinition NPLSelf = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("NPLFunctionName")]
        internal static ClassificationTypeDefinition NPLFunctionName = null;

    }
}
