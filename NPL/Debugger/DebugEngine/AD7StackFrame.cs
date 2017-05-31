using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace NPLTools.Debugger.DebugEngine
{
    // Represents a logical stack frame on the thread stack. 
    // Also implements the IDebugExpressionContext interface, which allows expression evaluation and watch windows.
    public class AD7StackFrame : IDebugStackFrame2, IDebugExpressionContext2//, IDebugProperty2
    {
        private readonly AD7Engine _engine;
        private readonly AD7Thread _thread;
        private int _lineNo;
        private string _fileName;
        public AD7StackFrame(AD7Engine engine, AD7Thread thread, string filename, int lineNo)
        {
            _engine = engine;
            _thread = thread;
            _lineNo = lineNo; //debug only
            _fileName = filename;
        }

        public void SetFrameInfo(enum_FRAMEINFO_FLAGS dwFieldSpec, out FRAMEINFO frameInfo)
        {
            frameInfo = new FRAMEINFO();

            // The debugger is asking for the formatted name of the function which is displayed in the callstack window.
            // There are several optional parts to this name including the module, argument types and values, and line numbers.
            // The optional information is requested by setting flags in the dwFieldSpec parameter.
            if ((dwFieldSpec & enum_FRAMEINFO_FLAGS.FIF_FUNCNAME) != 0)
            {
                string funcName = Path.GetFileName(_fileName) + " " + _lineNo;

                frameInfo.m_bstrFuncName = funcName;
                frameInfo.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_FUNCNAME;

                if ((dwFieldSpec & enum_FRAMEINFO_FLAGS.FIF_FUNCNAME_LINES) != 0)
                {
                    frameInfo.m_bstrFuncName = funcName;
                    frameInfo.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_FUNCNAME_LINES;
                }
            }

            if ((dwFieldSpec & enum_FRAMEINFO_FLAGS.FIF_LANGUAGE) != 0)
            {
                frameInfo.m_bstrLanguage = NPLConstants.LanguageName;
                frameInfo.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_LANGUAGE;
            }

            // The debugger is requesting the name of the module for this stack frame.
            if ((dwFieldSpec & enum_FRAMEINFO_FLAGS.FIF_MODULE) != 0)
            {
                frameInfo.m_bstrModule = "module name";
                frameInfo.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_MODULE;
            }

            // The debugger is requesting the IDebugStackFrame2 value for this frame info.
            if ((dwFieldSpec & enum_FRAMEINFO_FLAGS.FIF_FRAME) != 0)
            {
                frameInfo.m_pFrame = this;
                frameInfo.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_FRAME;
            }

            // Does this stack frame of symbols loaded?
            if ((dwFieldSpec & enum_FRAMEINFO_FLAGS.FIF_DEBUGINFO) != 0)
            {
                frameInfo.m_fHasDebugInfo = 1;
                frameInfo.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_DEBUGINFO;
            }

            // Is this frame stale?
            if ((dwFieldSpec & enum_FRAMEINFO_FLAGS.FIF_STALECODE) != 0)
            {
                frameInfo.m_fStaleCode = 0;
                frameInfo.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_STALECODE;
            }

            // The debugger would like a pointer to the IDebugModule2 that contains this stack frame.
            if ((dwFieldSpec & enum_FRAMEINFO_FLAGS.FIF_DEBUG_MODULEP) != 0)
            {
                // TODO: Module                
                /*
                if (module != null)
                {
                    AD7Module ad7Module = (AD7Module)module.Client;
                    Debug.Assert(ad7Module != null);
                    frameInfo.m_pModule = ad7Module;
                    frameInfo.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_DEBUG_MODULEP;
                }*/
            }
        }

        int IDebugStackFrame2.EnumProperties(enum_DEBUGPROP_INFO_FLAGS dwFields, uint nRadix, ref Guid guidFilter, uint dwTimeout, out uint pcelt, out IEnumDebugPropertyInfo2 ppEnum)
        {
            throw new NotImplementedException();
        }

        int IDebugStackFrame2.GetCodeContext(out IDebugCodeContext2 memoryAddress)
        {
            memoryAddress = new AD7MemoryAddress(_engine, _fileName, (uint)_lineNo);
            return VSConstants.S_OK;
        }

        int IDebugStackFrame2.GetDebugProperty(out IDebugProperty2 ppProperty)
        {
            throw new NotImplementedException();
        }

        int IDebugStackFrame2.GetDocumentContext(out IDebugDocumentContext2 docContext)
        {
            docContext = null;
            // Assume all lines begin and end at the beginning of the line.
            TEXT_POSITION begTp = new TEXT_POSITION();
            begTp.dwColumn = 0;
            begTp.dwLine = (uint)_lineNo - 1;
            TEXT_POSITION endTp = new TEXT_POSITION();
            endTp.dwColumn = 0;
            endTp.dwLine = (uint)_lineNo - 1;

            docContext = new AD7DocumentContext(_fileName, begTp, endTp, null);
            return VSConstants.S_OK;
        }

        int IDebugStackFrame2.GetExpressionContext(out IDebugExpressionContext2 ppExprCxt)
        {
            throw new NotImplementedException();
        }

        int IDebugStackFrame2.GetInfo(enum_FRAMEINFO_FLAGS dwFieldSpec, uint nRadix, FRAMEINFO[] pFrameInfo)
        {
            SetFrameInfo(dwFieldSpec, out pFrameInfo[0]);
            return VSConstants.S_OK;
        }

        int IDebugStackFrame2.GetLanguageInfo(ref string pbstrLanguage, ref Guid pguidLanguage)
        {
            pbstrLanguage = NPLConstants.LanguageName;
            pguidLanguage = NPLGuids.guidLanguageNPL;
            return VSConstants.S_OK;
        }

        int IDebugExpressionContext2.GetName(out string pbstrName)
        {
            throw new NotImplementedException();
        }

        int IDebugStackFrame2.GetName(out string pbstrName)
        {
            throw new NotImplementedException();
        }

        int IDebugStackFrame2.GetPhysicalStackRange(out ulong paddrMin, out ulong paddrMax)
        {
            throw new NotImplementedException();
        }

        int IDebugStackFrame2.GetThread(out IDebugThread2 ppThread)
        {
            throw new NotImplementedException();
        }

        int IDebugExpressionContext2.ParseText(string pszCode, enum_PARSEFLAGS dwFlags, uint nRadix, out IDebugExpression2 ppExpr, out string pbstrError, out uint pichError)
        {
            throw new NotImplementedException();
        }
    }
}

