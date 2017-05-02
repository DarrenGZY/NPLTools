using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace NPLTools.Language.Classifier
{
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "NPLKeyword")]
    [Name("NPLKeyword")]
    [UserVisible(false)]
    [Order(Before = Priority.Default)]
    internal sealed class NPLKeywordFormat : ClassificationFormatDefinition
    {
        public NPLKeywordFormat()
        {
            DisplayName = "NPLKeyword"; 
            ForegroundColor = Colors.Blue;
        }
    }


    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "NPLIdentifier")]
    [Name("NPLIdentifier")]
    [UserVisible(false)]
    [Order(Before = Priority.Default)]
    internal sealed class NPLIdentifierFormat : ClassificationFormatDefinition
    {
        public NPLIdentifierFormat()
        {
            DisplayName = "NPLIdentifier"; 
            ForegroundColor = Colors.Black;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "NPLString")]
    [Name("NPLString")]
    [UserVisible(false)]
    [Order(Before = Priority.Default)]
    internal sealed class NPLStringFormat : ClassificationFormatDefinition
    {
        public NPLStringFormat()
        {
            DisplayName = "NPLString"; 
            ForegroundColor = Colors.Red;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "NPLComment")]
    [Name("NPLComment")]
    [UserVisible(false)]
    [Order(Before = Priority.Default)]
    internal sealed class NPLCommentFormat : ClassificationFormatDefinition
    {
        public NPLCommentFormat()
        {
            DisplayName = "NPLComment"; 
            ForegroundColor = Colors.DarkGreen;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "NPLText")]
    [Name("NPLText")]
    [UserVisible(false)]
    [Order(Before = Priority.Default)]
    internal sealed class NPLTextFormat : ClassificationFormatDefinition
    {
        public NPLTextFormat()
        {
            DisplayName = "NPLText"; 
            ForegroundColor = Colors.Black;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "NPLNumber")]
    [Name("NPLNumber")]
    [UserVisible(false)]
    [Order(Before = Priority.Default)]
    internal sealed class NPLNumber : ClassificationFormatDefinition
    {
        public NPLNumber()
        {
            DisplayName = "NPLNumber"; 
            ForegroundColor = Colors.GreenYellow;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "NPLSelf")]
    [Name("NPLSelf")]
    [UserVisible(false)]
    [Order(Before = Priority.Default)]
    internal sealed class NPLSelf : ClassificationFormatDefinition
    {
        public NPLSelf()
        {
            DisplayName = "NPLSelf";
            ForegroundColor = Colors.Tomato;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "NPLFunctionName")]
    [Name("NPLFunctionName")]
    [UserVisible(false)]
    [Order(Before = Priority.Default)]
    internal sealed class NPLFunctionName : ClassificationFormatDefinition
    {
        public NPLFunctionName()
        {
            DisplayName = "NPLNumber";
            ForegroundColor = Colors.DarkRed;
        }
    }
}
