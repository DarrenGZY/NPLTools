//***************************************************************************
// 
//    Copyright (c) Microsoft Corporation. All rights reserved.
//    This code is licensed under the Visual Studio SDK license terms.
//    THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//    ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//    IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//    PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//***************************************************************************

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace NPL.Classifier
{
    internal static class NPLClassificationDefinition
    {
        #region Type definition

        /// <summary>
        /// Defines the keyword classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("Keyword")]
        internal static ClassificationTypeDefinition NPLKeyword = null;

        /// <summary>
        /// Defines the identifier classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("Id")]
        internal static ClassificationTypeDefinition NPLIdentifier = null;

        /// <summary>
        /// Defines the string classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("String")]
        internal static ClassificationTypeDefinition NPLString = null;
        
        /// <summary>
        /// Defines the comment classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("Comment")]
        internal static ClassificationTypeDefinition NPLComment = null;

        /// <summary>
        /// Defines the string classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("Text")]
        internal static ClassificationTypeDefinition NPLText = null;

        /// <summary>
        /// Defines the number classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("Number")]
        internal static ClassificationTypeDefinition NPLNumber = null;


        #endregion
    }
}
