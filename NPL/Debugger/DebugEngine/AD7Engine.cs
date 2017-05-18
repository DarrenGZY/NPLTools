using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Debugger.Interop;
using System.Diagnostics;
using System.Threading;
using System.Linq;
using Microsoft.VisualStudio;

namespace NPLTools.Debugger.DebugEngine
{
    // AD7Engine is the primary entrypoint object for the sample engine. 
    //
    // It implements:
    //
    // IDebugEngine2: This interface represents a debug engine (DE). It is used to manage various aspects of a debugging session, 
    // from creating breakpoints to setting and clearing exceptions.
    //
    // IDebugEngineLaunch2: Used by a debug engine (DE) to launch and terminate programs.
    //
    // IDebugProgram3: This interface represents a program that is running in a process. Since this engine only debugs one process at a time and each 
    // process only contains one program, it is implemented on the engine.
    //
    // IDebugEngineProgram2: This interface provides simultanious debugging of multiple threads in a debuggee.
    
    [ComVisible(true)]
    [Guid("8355452D-6D2F-41b0-89B8-BB2AA2529E94")]
    public class AD7Engine : IDebugEngine2, IDebugEngineLaunch2, IDebugProgram3, IDebugEngineProgram2, IDebugSymbolSettings100
    {
        // used to send events to the debugger. Some examples of these events are thread create, exception thrown, module load.
        //EngineCallback m_engineCallback; 

        // The sample debug engine is split into two parts: a managed front-end and a mixed-mode back end. DebuggedProcess is the primary
        // object in the back-end. AD7Engine holds a reference to it.
        //DebuggedProcess m_debuggedProcess;

        // This object facilitates calling from this thread into the worker thread of the engine. This is necessary because the Win32 debugging
        // api requires thread affinity to several operations.
        //WorkerThread m_pollThread;
        private Dictionary<LuaThread, AD7Thread> _threads = new Dictionary<LuaThread, AD7Thread>();
        private Dictionary<LuaModule, AD7Module> _modules = new Dictionary<LuaModule, AD7Module>();

        public const string DebugEngineId = NPLGuids.EngineId;
        public const string DebugEngineName = NPLConstants.DebugEngineName;
        // This object manages breakpoints in the sample engine.
        private BreakpointManager _breakpointManager;

        private LuaProcess _process;

        private IDebugEventCallback2 _ad7Callback;

        // A unique identifier for the program being debugged.
        private Guid _ad7ProgramId;

        public LuaProcess Process
        {
            get
            {
                return _process;
            }
        }

        public BreakpointManager BreakpointManager => _breakpointManager;

        public AD7Engine()
        {
            _breakpointManager = new BreakpointManager(this);
        }

        ~AD7Engine()
        { 

        }
        
        #region IDebugEngine2 Members

        // Attach the debug engine to a program. 
        int IDebugEngine2.Attach(IDebugProgram2[] rgpPrograms, IDebugProgramNode2[] rgpProgramNodes, uint celtPrograms, IDebugEventCallback2 ad7Callback, enum_ATTACH_REASON dwReason)
        {
            var program = rgpPrograms[0];
            int processId = EngineUtils.GetProcessId(program);
            if (processId == 0)
            {
                // engine only supports system processes
                Debug.WriteLine("LuaEngine failed to get process id during attach");
                return VSConstants.E_NOTIMPL;
            }

            EngineUtils.RequireOk(program.GetProgramId(out _ad7ProgramId));

            AD7EngineCreateEvent.Send(this);
            AD7ProgramCreateEvent.Send(this);

            Debug.WriteLine("LuaEngine Attach returning S_OK");
            return VSConstants.S_OK;
        }

        // Requests that all programs being debugged by this DE stop execution the next time one of their threads attempts to run.
        // This is normally called in response to the user clicking on the pause button in the debugger.
        // When the break is complete, an AsyncBreakComplete event will be sent back to the debugger.
        int IDebugEngine2.CauseBreak()
        {
            return ((IDebugProgram2)this).CauseBreak();
        }

        // Called by the SDM to indicate that a synchronous debug event, previously sent by the DE to the SDM,
        // was received and processed. The only event the sample engine sends in this fashion is Program Destroy.
        // It responds to that event by shutting down the engine.
        int IDebugEngine2.ContinueFromSynchronousEvent(IDebugEvent2 eventObject)
        {
            return VSConstants.S_OK;
        }

        // Creates a pending breakpoint in the engine. A pending breakpoint is contains all the information needed to bind a breakpoint to 
        // a location in the debuggee.
        int IDebugEngine2.CreatePendingBreakpoint(IDebugBreakpointRequest2 pBPRequest, out IDebugPendingBreakpoint2 ppPendingBP)
        {
            ppPendingBP = null;
            _breakpointManager.CreatePendingBreakpoint(pBPRequest, out ppPendingBP);
            return VSConstants.S_OK;
        }

        // Informs a DE that the program specified has been atypically terminated and that the DE should 
        // clean up all references to the program and send a program destroy event.
        int IDebugEngine2.DestroyProgram(IDebugProgram2 pProgram)
        {
            // Tell the SDM that the engine knows that the program is exiting, and that the
            // engine will send a program destroy. We do this because the Win32 debug api will always
            // tell us that the process exited, and otherwise we have a race condition.

            return (AD7_HRESULT.E_PROGRAM_DESTROY_PENDING);
        }

        // Gets the GUID of the DE.
        int IDebugEngine2.GetEngineId(out Guid guidEngine)
        {
            guidEngine = NPLGuids.EngineGuid;
            return VSConstants.S_OK;
        }

        // Removes the list of exceptions the IDE has set for a particular run-time architecture or language.
        // The sample engine does not support exceptions in the debuggee so this method is not actually implemented.
        int IDebugEngine2.RemoveAllSetExceptions(ref Guid guidType)
        {
            return VSConstants.S_OK;
        }

        // Removes the specified exception so it is no longer handled by the debug engine.
        // The sample engine does not support exceptions in the debuggee so this method is not actually implemented.       
        int IDebugEngine2.RemoveSetException(EXCEPTION_INFO[] pException)
        {
            // The sample engine will always stop on all exceptions.

            return VSConstants.S_OK;
        }

        // Specifies how the DE should handle a given exception.
        // The sample engine does not support exceptions in the debuggee so this method is not actually implemented.
        int IDebugEngine2.SetException(EXCEPTION_INFO[] pException)
        {           
            return VSConstants.S_OK;
        }

        // Sets the locale of the DE.
        // This method is called by the session debug manager (SDM) to propagate the locale settings of the IDE so that
        // strings returned by the DE are properly localized. The sample engine is not localized so this is not implemented.
        int IDebugEngine2.SetLocale(ushort wLangID)
        {
            return VSConstants.S_OK;
        }

        // A metric is a registry value used to change a debug engine's behavior or to advertise supported functionality. 
        // This method can forward the call to the appropriate form of the Debugging SDK Helpers function, SetMetric.
        int IDebugEngine2.SetMetric(string pszMetric, object varValue)
        {
            // The sample engine does not need to understand any metric settings.
            return VSConstants.S_OK;
        }

        // Sets the registry root currently in use by the DE. Different installations of Visual Studio can change where their registry information is stored
        // This allows the debugger to tell the engine where that location is.
        int IDebugEngine2.SetRegistryRoot(string pszRegistryRoot)
        {
            // The sample engine does not read settings from the registry.
            return VSConstants.S_OK;
        }

        #endregion

        #region IDebugEngineLaunch2 Members

        // Determines if a process can be terminated.
        int IDebugEngineLaunch2.CanTerminateProcess(IDebugProcess2 process)
        {
            return VSConstants.S_OK;
        }

        // Launches a process by means of the debug engine.
        // Normally, Visual Studio launches a program using the IDebugPortEx2::LaunchSuspended method and then attaches the debugger 
        // to the suspended program. However, there are circumstances in which the debug engine may need to launch a program 
        // (for example, if the debug engine is part of an interpreter and the program being debugged is an interpreted language), 
        // in which case Visual Studio uses the IDebugEngineLaunch2::LaunchSuspended method
        // The IDebugEngineLaunch2::ResumeProcess method is called to start the process after the process has been successfully launched in a suspended state.
        int IDebugEngineLaunch2.LaunchSuspended(string pszServer, IDebugPort2 port, string exe, string args, string dir, string env, string options, enum_LAUNCH_FLAGS launchFlags, uint hStdInput, uint hStdOutput, uint hStdError, IDebugEventCallback2 ad7Callback, out IDebugProcess2 process)
        {
            process = null;

            _ad7Callback = ad7Callback;

            _process = new LuaProcess(exe, args, dir, env);
            _process.ModuleLoad += OnModuleLoad;
            _process.BreakPointHit += OnBreakPointHit;
            _process.Start();

            TaskHelpers.RunSynchronouslyOnUIThread(ct => _process.StartListeningAsync());

            AD_PROCESS_ID adProcessId = new AD_PROCESS_ID();
            adProcessId.ProcessIdType = (uint)enum_AD_PROCESS_ID.AD_PROCESS_ID_SYSTEM;
            adProcessId.dwProcessId = (uint)_process.Id;

            port.GetProcess(adProcessId, out process);

            return VSConstants.S_OK;
        }

        // Resume a process launched by IDebugEngineLaunch2.LaunchSuspended
        int IDebugEngineLaunch2.ResumeProcess(IDebugProcess2 process)
        {
            int processId = EngineUtils.GetProcessId(process);

            if (processId != _process.Id)
            {
                Debug.WriteLine("ResumeProcess fails, wrong process");
                return VSConstants.S_FALSE;
            }

            // Send a program node to the SDM. This will cause the SDM to turn around and call IDebugEngine2.Attach
            // which will complete the hookup with AD7
            IDebugPort2 port;
            EngineUtils.RequireOk(process.GetPort(out port));

            IDebugDefaultPort2 defaultPort = (IDebugDefaultPort2)port;

            IDebugPortNotify2 portNotify;
            EngineUtils.RequireOk(defaultPort.GetPortNotify(out portNotify));

            EngineUtils.RequireOk(portNotify.AddProgramNode(new AD7ProgramNode(_process.Id)));

            if (_ad7ProgramId == Guid.Empty)
            {
                Debug.WriteLine("ResumeProcess fails, empty program guid");
                Debug.Fail("Unexpected problem -- IDebugEngine2.Attach wasn't called");
                return VSConstants.E_FAIL;
            }

            // debug only, use OnThreadCreat()
            //var luaModule = new LuaModule(0, "test.lua");
            //var adModule = new AD7Module(luaModule);
            //_modules.Add(luaModule, adModule);
            //SendModuleLoaded(adModule);

            var luaThread = new LuaThread(0, false);
            var newThread = new AD7Thread(this, new LuaThread(0, false));
            _threads.Add(luaThread, newThread);
            Send(new AD7ThreadCreateEvent(), AD7ThreadCreateEvent.IID, newThread);

            Debug.WriteLine("ResumeProcess return S_OK");
            return VSConstants.S_OK;
        }

        // This function is used to terminate a process that the SampleEngine launched
        // The debugger will call IDebugEngineLaunch2::CanTerminateProcess before calling this method.
        int IDebugEngineLaunch2.TerminateProcess(IDebugProcess2 process)
        {
            return VSConstants.S_OK;
        }

        #endregion

        #region IDebugProgram2 Members

        // Determines if a debug engine (DE) can detach from the program.
        public int CanDetach()
        {
            // The sample engine always supports detach
            return VSConstants.S_OK;
        }

        // The debugger calls CauseBreak when the user clicks on the pause button in VS. The debugger should respond by entering
        // breakmode. 
        public int CauseBreak()
        {
            return VSConstants.S_OK;
        }

        // Continue is called from the SDM when it wants execution to continue in the debugee
        // but have stepping state remain. An example is when a tracepoint is executed, 
        // and the debugger does not want to actually enter break mode.
        public int Continue(IDebugThread2 pThread)
        {
            return VSConstants.S_OK;
        }

        // Detach is called when debugging is stopped and the process was attached to (as opposed to launched)
        // or when one of the Detach commands are executed in the UI.
        public int Detach()
        {
            return VSConstants.S_OK;
        }

        // Enumerates the code contexts for a given position in a source file.
        public int EnumCodeContexts(IDebugDocumentPosition2 pDocPos, out IEnumDebugCodeContexts2 ppEnum)
        {
            throw new NotImplementedException();
        }

        // EnumCodePaths is used for the step-into specific feature -- right click on the current statment and decide which
        // function to step into. This is not something that the SampleEngine supports.
        public int EnumCodePaths(string hint, IDebugCodeContext2 start, IDebugStackFrame2 frame, int fSource, out IEnumCodePaths2 pathEnum, out IDebugCodeContext2 safetyContext)
        {
            pathEnum = null;
            safetyContext = null;
            return VSConstants.E_NOTIMPL;
        }

        // EnumModules is called by the debugger when it needs to enumerate the modules in the program.
        public int EnumModules(out IEnumDebugModules2 ppEnum)
        {
            AD7Module[] moduleObjects = new AD7Module[_modules.Count];
            int i = 0;
            foreach (var keyValue in _modules)
            {
                var module = keyValue.Key;
                var adModule = keyValue.Value;

                moduleObjects[i++] = adModule;
            }

            ppEnum = new AD7ModuleEnum(moduleObjects);
            return VSConstants.S_OK;
        }

        // EnumThreads is called by the debugger when it needs to enumerate the threads in the program.
        public int EnumThreads(out IEnumDebugThreads2 ppEnum)
        {
            AD7Thread[] threadObjects = new AD7Thread[_threads.Count];
            int i = 0;
            foreach (var keyValue in _threads)
            {
                var thread = keyValue.Key;
                var adThread = keyValue.Value;

                Debug.Assert(adThread != null);
                threadObjects[i++] = adThread;
            }

            ppEnum = new AD7ThreadEnum(threadObjects);

            return VSConstants.S_OK;
        }

        // The properties returned by this method are specific to the program. If the program needs to return more than one property, 
        // then the IDebugProperty2 object returned by this method is a container of additional properties and calling the 
        // IDebugProperty2::EnumChildren method returns a list of all properties.
        // A program may expose any number and type of additional properties that can be described through the IDebugProperty2 interface. 
        // An IDE might display the additional program properties through a generic property browser user interface.
        // The sample engine does not support this
        public int GetDebugProperty(out IDebugProperty2 ppProperty)
        {
            throw new NotImplementedException();
        }

        // The debugger calls this when it needs to obtain the IDebugDisassemblyStream2 for a particular code-context.
        // The sample engine does not support dissassembly so it returns E_NOTIMPL
        public int GetDisassemblyStream(enum_DISASSEMBLY_STREAM_SCOPE dwScope, IDebugCodeContext2 codeContext, out IDebugDisassemblyStream2 disassemblyStream)
        {
            disassemblyStream = null;
            return VSConstants.E_NOTIMPL;
        }

        // This method gets the Edit and Continue (ENC) update for this program. A custom debug engine always returns E_NOTIMPL
        public int GetENCUpdate(out object update)
        {
            // The sample engine does not participate in managed edit & continue.
            update = null;
            return VSConstants.S_OK;            
        }

        // Gets the name and identifier of the debug engine (DE) running this program.
        public int GetEngineInfo(out string engineName, out Guid engineGuid)
        {
            engineName = NPLConstants.DebugEngineName;
            engineGuid = NPLGuids.EngineGuid;
            return VSConstants.S_OK;
        }

        // The memory bytes as represented by the IDebugMemoryBytes2 object is for the program's image in memory and not any memory 
        // that was allocated when the program was executed.
        public int GetMemoryBytes(out IDebugMemoryBytes2 ppMemoryBytes)
        {
            throw new NotImplementedException();
        }

        // Gets the name of the program.
        // The name returned by this method is always a friendly, user-displayable name that describes the program.
        public int GetName(out string programName)
        {
            // The Sample engine uses default transport and doesn't need to customize the name of the program,
            // so return NULL.
            programName = null;
            return VSConstants.S_OK;
        }

        // Gets a GUID for this program. A debug engine (DE) must return the program identifier originally passed to the IDebugProgramNodeAttach2::OnAttach
        // or IDebugEngine2::Attach methods. This allows identification of the program across debugger components.
        public int GetProgramId(out Guid guidProgramId)
        {
            Debug.Assert(_ad7ProgramId != Guid.Empty);

            guidProgramId = _ad7ProgramId;
            return VSConstants.S_OK;
        }

        // This method is deprecated. Use the IDebugProcess3::Step method instead.
        public int Step(IDebugThread2 pThread, enum_STEPKIND sk, enum_STEPUNIT Step)
        {
            return VSConstants.S_OK;
        }

        // Terminates the program.
        public int Terminate()
        {
            // Because the sample engine is a native debugger, it implements IDebugEngineLaunch2, and will terminate
            // the process in IDebugEngineLaunch2.TerminateProcess
            return VSConstants.S_OK;
        }

        // Writes a dump to a file.
        public int WriteDump(enum_DUMPTYPE DUMPTYPE, string pszDumpUrl)
        {
            // The sample debugger does not support creating or reading mini-dumps.
            return VSConstants.E_NOTIMPL;
        }

        #endregion

        #region IDebugProgram3 Members

        // ExecuteOnThread is called when the SDM wants execution to continue and have 
        // stepping state cleared.
        public int ExecuteOnThread(IDebugThread2 pThread)
        {

            return VSConstants.S_OK;
        }

        #endregion

        #region IDebugEngineProgram2 Members

        // Stops all threads running in this program.
        // This method is called when this program is being debugged in a multi-program environment. When a stopping event from some other program 
        // is received, this method is called on this program. The implementation of this method should be asynchronous; 
        // that is, not all threads should be required to be stopped before this method returns. The implementation of this method may be 
        // as simple as calling the IDebugProgram2::CauseBreak method on this program.
        //
        // The sample engine only supports debugging native applications and therefore only has one program per-process
        public int Stop()
        {
            throw new NotImplementedException();
        }

        // WatchForExpressionEvaluationOnThread is used to cooperate between two different engines debugging 
        // the same process. The sample engine doesn't cooperate with other engines, so it has nothing
        // to do here.
        public int WatchForExpressionEvaluationOnThread(IDebugProgram2 pOriginatingProgram, uint dwTid, uint dwEvalFlags, IDebugEventCallback2 pExprCallback, int fWatch)
        {
            return VSConstants.S_OK;
        }

        // WatchForThreadStep is used to cooperate between two different engines debugging the same process.
        // The sample engine doesn't cooperate with other engines, so it has nothing to do here.
        public int WatchForThreadStep(IDebugProgram2 pOriginatingProgram, uint dwTid, int fWatch, uint dwFrame)
        {
            return VSConstants.S_OK;
        }

        #endregion

        #region IDebugSymbolSettings100 members
        public int SetSymbolLoadState(int bIsManual, int bLoadAdjacent, string strIncludeList, string strExcludeList)
        {
            // The SDM will call this method on the debug engine when it is created, to notify it of the user's
            // symbol settings in Tools->Options->Debugging->Symbols.
            //
            // Params:
            // bIsManual: true if 'Automatically load symbols: Only for specified modules' is checked
            // bLoadAdjacent: true if 'Specify modules'->'Always load symbols next to the modules' is checked
            // strIncludeList: semicolon-delimited list of modules when automatically loading 'Only specified modules'
            // strExcludeList: semicolon-delimited list of modules when automatically loading 'All modules, unless excluded'

            return VSConstants.S_OK;
        }
        #endregion

        #region Events
        internal void Send(IDebugEvent2 eventObject, string iidEvent, IDebugProgram2 program, IDebugThread2 thread)
        {
            uint attributes;
            Guid riidEvent = new Guid(iidEvent);

            EngineUtils.RequireOk(eventObject.GetAttributes(out attributes));

            var ad7Callback = _ad7Callback;
            if (ad7Callback == null)
            {
                // Probably racing with the end of the process.
                Debug.Fail("_events is null");
                return;
            }

            Debug.WriteLine(String.Format("Sending Event: {0} {1}", eventObject.GetType(), iidEvent));
            try
            {
                EngineUtils.RequireOk(ad7Callback.Event(this, null, program, thread, eventObject, ref riidEvent, attributes));
            }
            catch (InvalidCastException)
            {
                // COM object has gone away
            }
        }

        internal void Send(IDebugEvent2 eventObject, string iidEvent, IDebugThread2 thread)
        {
            Send(eventObject, iidEvent, this, thread);
        }

        private void OnBreakPointHit(object sender, BreakpointEventArgs e)
        {
            var boundBreakpoints = new[] { _breakpointManager.GetBreakpoint(e.Breakpoint) };

            // An engine that supports more advanced breakpoint features such as hit counts, conditions and filters
            // should notify each bound breakpoint that it has been hit and evaluate conditions here.

            Send(new AD7BreakpointEvent(new AD7BoundBreakpointsEnum(boundBreakpoints)), AD7BreakpointEvent.IID, _threads.First().Value);
        }

        public void OnBreakpointBound(AD7PendingBreakpoint pendingBreakpoint, AD7BoundBreakpoint boundBreakpoint)
        {
            AD7BreakpointBoundEvent eventObject = new AD7BreakpointBoundEvent(pendingBreakpoint, boundBreakpoint);
            Send(eventObject, AD7BreakpointBoundEvent.IID, null);
        }

        private void OnModuleLoad(object sender, ModuleLoadEventArgs e)
        {
            var adModule = new AD7Module(e.Module);
            SendModuleLoaded(adModule);
        }

        private void SendModuleLoaded(AD7Module ad7Module)
        {
            AD7ModuleLoadEvent eventObject = new AD7ModuleLoadEvent(ad7Module, true);

            Send(eventObject, AD7ModuleLoadEvent.IID, null);
        }

        private void OnThreadCreated(object sender, EventArgs e)
        {
            var newThread = new AD7Thread(this, new LuaThread(0, false));
            Send(new AD7ThreadCreateEvent(), AD7ThreadCreateEvent.IID, newThread);
        }
        #endregion


        #region Deprecated interface methods
        // These methods are not called by the Visual Studio debugger, so they don't need to be implemented

        int IDebugEngine2.EnumPrograms(out IEnumDebugPrograms2 programs)
        {
            Debug.Fail("This function is not called by the debugger");

            programs = null;
            return VSConstants.E_NOTIMPL;
        }

        public int Attach(IDebugEventCallback2 pCallback)
        {
            Debug.Fail("This function is not called by the debugger");

            return VSConstants.E_NOTIMPL;
        }

        public int GetProcess(out IDebugProcess2 process)
        {
            Debug.Fail("This function is not called by the debugger");

            process = null;
            return VSConstants.E_NOTIMPL;
        }

        public int Execute()
        {
            Debug.Fail("This function is not called by the debugger.");
            return VSConstants.E_NOTIMPL;
        }

        #endregion
      
    }
}
