using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using System.Diagnostics;
using System;
using IServiceProvider = System.IServiceProvider;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using NPLTools.Intelligense;
using Microsoft.VisualStudio.ComponentModelHost;
using NPLTools.Intelligense2;

namespace NPLTools.Language
{
    internal class NPLEditorCommandFilter : IOleCommandTarget
    {
        private readonly ITextView _textView;
        private readonly IVsTextView _vsTextView;
        private readonly IServiceProvider _serviceProvider;
        private readonly IComponentModel _componentModel;

        public NPLEditorCommandFilter(ITextView textView, IVsTextView vsTextView, IServiceProvider serviceProvider)
        {
            _textView = textView;
            _vsTextView = vsTextView;
            _serviceProvider = serviceProvider;
        }
    
        public IOleCommandTarget Next { get; set; }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (pguidCmdGroup == VsMenus.guidStandardCommandSet2K)
            {
                switch ((VSConstants.VSStd2KCmdID)nCmdID)
                {
                    case VSConstants.VSStd2KCmdID.COMMENT_BLOCK:
                    case VSConstants.VSStd2KCmdID.COMMENTBLOCK:
                        CommentOrUncommentBlock(true);
                        break;
                    case VSConstants.VSStd2KCmdID.UNCOMMENT_BLOCK:
                    case VSConstants.VSStd2KCmdID.UNCOMMENTBLOCK:
                        CommentOrUncommentBlock(false);
                        break;
                    case VSConstants.VSStd2KCmdID.FORMATSELECTION:
                        FormatBlock();
                        break;
                }
            }
            else if (pguidCmdGroup == VsMenus.guidStandardCommandSet97)
            {
                switch ((VSConstants.VSStd97CmdID)nCmdID)
                {
                    case VSConstants.VSStd97CmdID.GotoDefn:
                        GotoDefinition();
                        break;
                }
            }

            return Next.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (pguidCmdGroup == VsMenus.guidStandardCommandSet2K)
            {
                for (int i = 0; i < cCmds; i++)
                {
                    switch ((VSConstants.VSStd2KCmdID)prgCmds[i].cmdID)
                    {
                        case VSConstants.VSStd2KCmdID.COMMENT_BLOCK:
                        case VSConstants.VSStd2KCmdID.COMMENTBLOCK:
                        case VSConstants.VSStd2KCmdID.UNCOMMENT_BLOCK:
                        case VSConstants.VSStd2KCmdID.UNCOMMENTBLOCK:
                        case VSConstants.VSStd2KCmdID.FORMATSELECTION:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                            return VSConstants.S_OK;
                    }
                }
            }
            else if (pguidCmdGroup == VsMenus.guidStandardCommandSet97)
            {
                for (int i = 0; i < cCmds; i++)
                {
                    switch ((VSConstants.VSStd97CmdID)prgCmds[i].cmdID)
                    {
                        case VSConstants.VSStd97CmdID.GotoDefn:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                            return VSConstants.S_OK;
                    }
                }
            }
            return Next.QueryStatus(pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        private void GotoDefinition()
        {
            var caret = _textView.Caret.Position.BufferPosition;
            var analysis = _textView.GetAnalysisAtCaret(_serviceProvider);

            var span = analysis.Analyzer.GetDeclarationLocation(analysis, _textView, caret);
        }

        private void CommentOrUncommentBlock(bool comment)
        {

        }

        private void FormatBlock()
        {

        }
    }
}
