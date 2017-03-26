using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using System.Diagnostics;
using System;
using Microsoft.VisualStudio.Text.Editor;

namespace NPLTools.Language.Editor
{
    internal class EditorCommandFilter : IOleCommandTarget
    {
        public EditorCommandFilter(IWpfTextView textView)
        {
            TextView = textView;
        }

        public IWpfTextView TextView { get; private set; }

        public IOleCommandTarget Next { get; set; }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (pguidCmdGroup == VsMenus.guidStandardCommandSet2K)
            {
                switch ((VSConstants.VSStd2KCmdID)nCmdID)
                {
                    case VSConstants.VSStd2KCmdID.COMMENT_BLOCK:
                    case VSConstants.VSStd2KCmdID.COMMENTBLOCK:
                        CommentHelper.CommentOrUncommentBlock(TextView, true);
                        break;
                    case VSConstants.VSStd2KCmdID.UNCOMMENT_BLOCK:
                    case VSConstants.VSStd2KCmdID.UNCOMMENTBLOCK:
                        CommentHelper.CommentOrUncommentBlock(TextView, false);
                        break;
                    case VSConstants.VSStd2KCmdID.FORMATSELECTION:
                        Debug.WriteLine("Format Selection Command");
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
            return Next.QueryStatus(pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }
    }
}
