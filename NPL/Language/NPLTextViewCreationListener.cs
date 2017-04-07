using System;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.OLE.Interop;
using System.Runtime.InteropServices;
using Irony.Parsing;
using Irony.Interpreter.Ast;
using NPLTools.IronyParser.Ast;
using NPLTools.IronyParser;
using NPLTools.Intelligense;

namespace NPLTools.Language
{
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType("NPL")]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    class NPLTextViewCreationListener : IVsTextViewCreationListener, IDisposable
    {
        [Import]
        public IVsEditorAdaptersFactoryService AdaptersFactory { get; private set; }

        public static event EventHandler<NPLTextContentChangedEventArgs> TextContentChanged;
   
        private ITextView _view;
        private System.Threading.Timer _delayRefreshTimer;

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            _view = AdaptersFactory.GetWpfTextView(textViewAdapter);
            _view.TextBuffer.Changed += TextBuffer_Changed;

            IOleCommandTarget next;
            NPLEditorCommandFilter commandFilter = new NPLEditorCommandFilter(_view, textViewAdapter);
            textViewAdapter.AddCommandFilter(commandFilter, out next);
            commandFilter.Next = next;
        }

        private void TextBuffer_Changed(object sender, TextContentChangedEventArgs e)
        {
            if(_delayRefreshTimer != null)
            {
                _delayRefreshTimer.Dispose();
            }
            _delayRefreshTimer = new System.Threading.Timer(HandleTextChangedEvent, null, 500, System.Threading.Timeout.Infinite);    
        }

        private void HandleTextChangedEvent(object args)
        {
            OnTextContentChanged(_view.TextSnapshot);
        }

        private void OnTextContentChanged(ITextSnapshot snapShot)
        {
            TextContentChanged(this, new NPLTextContentChangedEventArgs(snapShot));
        } 

        public void Dispose()
        {
            if (_delayRefreshTimer != null)
            {
                _delayRefreshTimer.Dispose();
            }
        }
    }

    class NPLTextContentChangedEventArgs : EventArgs
    {
        public ITextSnapshot Snapshot { get; private set; }

        public NPLTextContentChangedEventArgs(ITextSnapshot Snapshot)
        {
            this.Snapshot = Snapshot;
        }
    }
}
