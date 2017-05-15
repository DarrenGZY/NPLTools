﻿using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio;

namespace NPLTools.Debugger.DebugEngine
{
    // This class represents a pending breakpoint which is an abstract representation of a breakpoint before it is bound.
    // When a user creates a new breakpoint, the pending breakpoint is created and is later bound. The bound breakpoints
    // become children of the pending breakpoint.
    class AD7PendingBreakpoint : IDebugPendingBreakpoint2
    {       
        // The breakpoint request that resulted in this pending breakpoint being created.
        private IDebugBreakpointRequest2 _bpRequest;
        private BP_REQUEST_INFO _bpRequestInfo; 
        private AD7Engine _engine;
        private BreakpointManager _bpManager;

        private System.Collections.Generic.List<AD7BoundBreakpoint> _boundBreakpoints;

        private bool _enabled;
        private bool _deleted;       

        public AD7PendingBreakpoint(IDebugBreakpointRequest2 pBPRequest, AD7Engine engine, BreakpointManager bpManager)
        {
            _bpRequest = pBPRequest;
            BP_REQUEST_INFO[] requestInfo = new BP_REQUEST_INFO[1];
            EngineUtils.CheckOk(_bpRequest.GetRequestInfo(enum_BPREQI_FIELDS.BPREQI_BPLOCATION | enum_BPREQI_FIELDS.BPREQI_CONDITION | enum_BPREQI_FIELDS.BPREQI_ALLFIELDS, requestInfo));
            _bpRequestInfo = requestInfo[0];            
            
            _engine = engine;
            _bpManager = bpManager;
            _boundBreakpoints = new System.Collections.Generic.List<AD7BoundBreakpoint>();

            _enabled = true;
            _deleted = false;
        }

        private bool CanBind()
        {
            // The sample engine only supports breakpoints on a file and line number. No other types of breakpoints are supported.
            if (this._deleted || _bpRequestInfo.bpLocation.bpLocationType != (uint)enum_BP_LOCATION_TYPE.BPLT_CODE_FILE_LINE)
            {
                return false;
            }

            return true;
        }

        // Get the document context for this pending breakpoint. A document context is a abstract representation of a source file 
        // location.
        public AD7DocumentContext GetDocumentContext(uint address)
        {
            IDebugDocumentPosition2 docPosition = (IDebugDocumentPosition2)(Marshal.GetObjectForIUnknown(_bpRequestInfo.bpLocation.unionmember2));
            string documentName = "";


            // Get the location in the document that the breakpoint is in.
            TEXT_POSITION[] startPosition = new TEXT_POSITION[1];
            TEXT_POSITION[] endPosition = new TEXT_POSITION[1];
         

            AD7MemoryAddress codeContext = new AD7MemoryAddress(_engine, address);
            
            return new AD7DocumentContext(documentName, startPosition[0], startPosition[0], codeContext);
        }

        // Remove all of the bound breakpoints for this pending breakpoint
        public void ClearBoundBreakpoints()
        {
            lock (_boundBreakpoints)
            {
                for (int i = _boundBreakpoints.Count - 1; i >= 0; i--)
                {
                    ((IDebugBoundBreakpoint2)_boundBreakpoints[i]).Delete();
                }
            }
        }

        // Called by bound breakpoints when they are being deleted.
        public void OnBoundBreakpointDeleted(AD7BoundBreakpoint boundBreakpoint)
        {
            lock (_boundBreakpoints)
            {
                _boundBreakpoints.Remove(boundBreakpoint);
            }
        }

        #region IDebugPendingBreakpoint2 Members

        // Binds this pending breakpoint to one or more code locations.
        int IDebugPendingBreakpoint2.Bind()
        {
            if (CanBind())
            {
                IDebugDocumentPosition2 docPosition = (IDebugDocumentPosition2)(Marshal.GetObjectForIUnknown(_bpRequestInfo.bpLocation.unionmember2));

                // Get the name of the document that the breakpoint was put in
                string documentName;
                EngineUtils.CheckOk(docPosition.GetFileName(out documentName));

                // Get the location in the document that the breakpoint is in.
                TEXT_POSITION[] startPosition = new TEXT_POSITION[1];
                TEXT_POSITION[] endPosition = new TEXT_POSITION[1];
                EngineUtils.CheckOk(docPosition.GetRange(startPosition, endPosition));
                _engine.Process.SendRequest("STEP\n");
                _engine.Process.SendRequest("SETB " + @"C:\Users\Zhiyuan\Documents\NPL_Projects\NPLTools\NPL\test.lua" + " " + "10" + "\n");
                _engine.Process.SendRequest("RUN\n");
            }
            return VSConstants.S_OK;
        }

        // Determines whether this pending breakpoint can bind to a code location.
        int IDebugPendingBreakpoint2.CanBind(out IEnumDebugErrorBreakpoints2 ppErrorEnum)
        {
            ppErrorEnum = null;

            if (!CanBind())
            {
                // Called to determine if a pending breakpoint can be bound. 
                // The breakpoint may not be bound for many reasons such as an invalid location, an invalid expression, etc...
                // The sample engine does not support this, but a real world engine will want to return a valid enumeration of IDebugErrorBreakpoint2.
                // The debugger will then display information about why the breakpoint did not bind to the user.
                ppErrorEnum = null;
                return VSConstants.S_FALSE;
            }

            return VSConstants.S_OK;
        }

        // Deletes this pending breakpoint and all breakpoints bound from it.
        int IDebugPendingBreakpoint2.Delete()
        {
            lock (_boundBreakpoints)
            {
                for (int i = _boundBreakpoints.Count - 1; i >= 0; i--)
                {
                    ((IDebugBoundBreakpoint2)_boundBreakpoints[i]).Delete();
                }
            }

            return VSConstants.S_OK;
        }

        // Toggles the enabled state of this pending breakpoint.
        int IDebugPendingBreakpoint2.Enable(int fEnable)
        {
            lock (_boundBreakpoints)
            {
                _enabled = fEnable == 0 ? false : true;

                foreach (AD7BoundBreakpoint bp in _boundBreakpoints)
                {
                    ((IDebugBoundBreakpoint2)_boundBreakpoints).Enable(fEnable);
                }
            }

            return VSConstants.S_OK;
        }

        // Enumerates all breakpoints bound from this pending breakpoint
        int IDebugPendingBreakpoint2.EnumBoundBreakpoints(out IEnumDebugBoundBreakpoints2 ppEnum)
        {
            lock (_boundBreakpoints)
            {
                IDebugBoundBreakpoint2[] boundBreakpoints = _boundBreakpoints.ToArray();
                ppEnum = new AD7BoundBreakpointsEnum(boundBreakpoints);
            }
            return VSConstants.S_OK;
        }

        // Enumerates all error breakpoints that resulted from this pending breakpoint.
        int IDebugPendingBreakpoint2.EnumErrorBreakpoints(enum_BP_ERROR_TYPE bpErrorType, out IEnumDebugErrorBreakpoints2 ppEnum)
        {
            // Called when a pending breakpoint could not be bound. This may occur for many reasons such as an invalid location, an invalid expression, etc...
            // The sample engine does not support this, but a real world engine will want to send an instance of IDebugBreakpointErrorEvent2 to the
            // UI and return a valid enumeration of IDebugErrorBreakpoint2 from IDebugPendingBreakpoint2::EnumErrorBreakpoints. The debugger will then
            // display information about why the breakpoint did not bind to the user.
            ppEnum = null;
            return VSConstants.E_NOTIMPL;
        }

        // Gets the breakpoint request that was used to create this pending breakpoint
        int IDebugPendingBreakpoint2.GetBreakpointRequest(out IDebugBreakpointRequest2 ppBPRequest)
        {
            ppBPRequest = this._bpRequest;
            return VSConstants.S_OK;
        }

        // Gets the state of this pending breakpoint.
        int IDebugPendingBreakpoint2.GetState(PENDING_BP_STATE_INFO[] pState)
        {
            if (_deleted)
            {
                pState[0].state = (enum_PENDING_BP_STATE)enum_BP_STATE.BPS_DELETED;
            }
            else if (_enabled)
            {
                pState[0].state = (enum_PENDING_BP_STATE)enum_BP_STATE.BPS_ENABLED;
            }
            else if (!_enabled)
            {
                pState[0].state = (enum_PENDING_BP_STATE)enum_BP_STATE.BPS_DISABLED;
            }

            return VSConstants.S_OK;
        }

        // The sample engine does not support conditions on breakpoints.
        int IDebugPendingBreakpoint2.SetCondition(BP_CONDITION bpCondition)
        {
            throw new NotImplementedException();
        }

        // The sample engine does not support pass counts on breakpoints.
        int IDebugPendingBreakpoint2.SetPassCount(BP_PASSCOUNT bpPassCount)
        {
            throw new NotImplementedException();
        }

        // Toggles the virtualized state of this pending breakpoint. When a pending breakpoint is virtualized, 
        // the debug engine will attempt to bind it every time new code loads into the program.
        // The sample engine will does not support this.
        int IDebugPendingBreakpoint2.Virtualize(int fVirtualize)
        {
            return VSConstants.S_OK;
        }

        #endregion
    }
}
