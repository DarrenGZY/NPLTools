using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using NPLTools;

namespace Microsoft.PythonTools.Editor
{
    [Export(typeof(ISmartIndentProvider))]
    [ContentType(NPLConstants.ContentType)]
    public sealed class SmartIndentProvider : ISmartIndentProvider
    {

        [ImportingConstructor]
        internal SmartIndentProvider([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            
        }

        private sealed class Indent : ISmartIndent
        {
            private readonly ITextView _textView;
            private readonly SmartIndentProvider _provider;

            public Indent(SmartIndentProvider provider, ITextView view)
            {
                _provider = provider;
                _textView = view;
            }

            public int? GetDesiredIndentation(ITextSnapshotLine line)
            {
                return null;
            }

            public void Dispose()
            {
            }
        }

        public ISmartIndent CreateSmartIndent(ITextView textView)
        {
            return new Indent(this, textView);
        }
    }
}
