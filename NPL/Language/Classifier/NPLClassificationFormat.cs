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
            ForegroundColor = Colors.BlueViolet;
        }
    }


    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "Id")]
    [Name("Id")]
    [UserVisible(false)]
    [Order(Before = Priority.Default)]
    internal sealed class NPLIdentifierFormat : ClassificationFormatDefinition
    {
        public NPLIdentifierFormat()
        {
            DisplayName = "Id"; 
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
            ForegroundColor = Colors.DarkOrange;
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
    [ClassificationType(ClassificationTypeNames = "Text")]
    [Name("Text")]
    [UserVisible(false)]
    [Order(Before = Priority.Default)]
    internal sealed class NPLTextFormat : ClassificationFormatDefinition
    {
        public NPLTextFormat()
        {
            DisplayName = "Text"; 
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
}
