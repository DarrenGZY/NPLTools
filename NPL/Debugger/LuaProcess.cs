using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NPLTools.Debugger
{
    public class StateObject
    {
        // Client  socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BufferSize = 1024;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        // Received data string.  
        public StringBuilder sb = new StringBuilder();
    }

    public class LuaProcess
    {
        private readonly Process _process;

        private int _pid;
        //private Socket _socketHandler;
        private NetworkStream _networkStream;
        private Thread _eventThread;
        private int _breakpointCounter;
        private readonly AutoResetEvent _connectedEvent = new AutoResetEvent(false);
        private readonly Dictionary<int, LuaBreakpoint> _breakpoints = new Dictionary<int, LuaBreakpoint>();
        private LuaThread _thread;
        public int Id => _pid;
        public event EventHandler<ModuleLoadEventArgs> ModuleLoad;
        public event EventHandler<BreakpointEventArgs> BreakPointHit;
        public event EventHandler<FrameListEventArgs> FrameList;
        public event EventHandler<ThreadCreateEventArgs> ThreadCreate;
        public event EventHandler<ProcessExitedEventArgs> ProcessExit;

        public LuaProcess(string exe, string args, string dir, string env)
        {
            int listenerPort = -1;
            var socketSource = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            socketSource.Bind(new IPEndPoint(IPAddress.Loopback, 0));
            socketSource.Listen(0);
            
            listenerPort = ((IPEndPoint)socketSource.LocalEndPoint).Port;
            Debug.WriteLine("Listening for debug connections on port {0}", listenerPort);
            socketSource.BeginAccept(AcceptConnection, socketSource);

            var processInfo = new ProcessStartInfo(exe);
            processInfo.CreateNoWindow = false;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardOutput = false;
            processInfo.RedirectStandardInput = false;
            processInfo.WorkingDirectory = dir;
            processInfo.Arguments = @"C:\Users\Zhiyuan\Documents\NPL_Projects\NPLTools\NPL\visualstudio_lua_debugger.lua " + args + " " + listenerPort;
            _process = new Process();
            _process.StartInfo = processInfo;
            _process.EnableRaisingEvents = true;
            _process.Exited += OnProcessExit;
        }

        private void OnProcessExit(object sender, EventArgs e)
        {
            int exitCode = (_process != null && _process.HasExited) ? _process.ExitCode : -1;
            ProcessExit?.Invoke(this, new ProcessExitedEventArgs(exitCode));
        }

        private void AcceptConnection(IAsyncResult iar)
        {
            Socket socket;
            var socketSource = ((Socket)iar.AsyncState);
            try
            {
                socket = socketSource.EndAccept(iar);
            }
            catch (SocketException ex)
            {
                Debug.WriteLine("DebugConnectionListener socket failed");
                Debug.WriteLine(ex);
                return;
            }
            catch (ObjectDisposedException)
            {
                Debug.WriteLine("DebugConnectionListener socket closed");
                return;
            }

            _networkStream = new NetworkStream(socket, ownsSocket: true);

            _connectedEvent.Set();

            _eventThread = new Thread(EventHandlingThread);
            _eventThread.Name = "event handling thread";
            _eventThread.Start();
        }

        public void SendRequest(string request)
        {
            var contentBytes = Encoding.UTF8.GetBytes(request);

            _networkStream.Write(contentBytes, 0, contentBytes.Length);
            _networkStream.Flush();
        }

        public async void EventHandlingThread()
        {
            StreamReader reader = new StreamReader(_networkStream);
            while (true && !_process.HasExited)
            {
                string response = reader.ReadLine();
                // wait until a response with real content
                if (response == null || response == String.Empty) continue;
                await HandleResponse(response);
            }
        }

        private Task HandleResponse(string response)
        {
            return Task.Run(() => {
                JObject obj = JObject.Parse(response);
                switch (obj["name"].ToObject<string>())
                {
                    case ResponseType.BreakpointHit:
                        {
                            var breakpoitnId = obj["id"].ToObject<int>();
                            LuaBreakpoint breakpoint;
                            if (_breakpoints.TryGetValue(breakpoitnId, out breakpoint))
                                BreakPointHit?.Invoke(this, new BreakpointEventArgs(_thread, breakpoint));
                            break;
                        }
                    case ResponseType.ModuleLoad:
                        {
                            var filename = obj["filename"].ToObject<string>();
                            var id = obj["id"].ToObject<int>();
                            LuaModule module = new LuaModule(id, filename);
                            ModuleLoad?.Invoke(this, new ModuleLoadEventArgs(module));
                            break;
                        }
                    case ResponseType.FrameList:
                        {
                            List<LuaStackFrame> frames = new List<LuaStackFrame>();
                            Frame currentFrame = obj["frame"].ToObject<Frame>();
                            while (currentFrame.FileName != null && currentFrame.LineNo != 0)
                            {
                                LuaStackFrame frame = new LuaStackFrame(currentFrame.FileName, currentFrame.LineNo);
                                frames.Add(frame);
                                currentFrame = currentFrame.Parent;
                            }
                            _thread.Frames = frames;
                            //FrameList?.Invoke(this, new FrameListEventArgs(_thread));
                            break;
                        }
                    case ResponseType.ThreadCreat:
                        {
                            _thread = new LuaThread(0, false);
                            ThreadCreate?.Invoke(this, new ThreadCreateEventArgs(_thread));
                            break;
                        }
                    case ResponseType.DebugExit:
                        {

                            break;
                        }
                    default:
                        break;
                }
            });
            
        }

        public void Start()
        {
            try
            {
                bool success = _process.Start();
                _pid = _process.Id;
            }
            catch (Exception e)
            {

            }
        }

        public void Terminate()
        {
            if (_process != null && !_process.HasExited)
            {
                _process.Kill();
            }
        }

        /// <summary>
        /// Starts listening for debugger communication.  Can be called after Start
        /// to give time to attach to debugger events.  This waits for the debuggee
        /// to connect to the socket.
        /// </summary>
        public async Task StartListeningAsync(int timeOutMs = 20000)
        {
            if (!_connectedEvent.WaitOne(timeOutMs))
            {
                throw new Exception("Connection timeout");
            }
        }

        #region breakpoints related functions
        public LuaBreakpoint AddBreakpoint(string filename, int lineNo)
        {
            int id = _breakpointCounter++;
            var res = new LuaBreakpoint(this, filename, lineNo,  id);
            _breakpoints[id] = res;
            return res;
        }

        public void Resume()
        {
            SendRequest(RequesetMessages.Run());
        }

        internal async Task BindBreakpointAsync(LuaBreakpoint breakpoint, CancellationToken ct)
        {
            //SendRequest("BIND breakpoint\n");
        }

        internal async Task SetBreakpointConditionAsync(LuaBreakpoint breakpoint, CancellationToken ct)
        {
            //SendRequest("SET breakpoint condition\n");
        }

        internal async Task SetBreakpointPassCountAsync(LuaBreakpoint breakpoint, CancellationToken ct)
        {
            //SendRequest("SET breakpoint passcount\n");
        }

        internal async Task SetBreakpointHitCountAsync(LuaBreakpoint breakpoint, int count, CancellationToken ct)
        {
            //SendRequest("SET breakpoint hit count\n");
        }

        internal async Task<int> GetBreakpointHitCountAsync(LuaBreakpoint breakpoint, CancellationToken ct)
        {
            //SendRequest("SET breakpoint hit count\n");
            return 0;
        }

        internal async Task RemoveBreakpointAsync(LuaBreakpoint unboundBreakpoint, CancellationToken ct)
        {
            _breakpoints.Remove(unboundBreakpoint.Id);
            await DisableBreakpointAsync(unboundBreakpoint, ct);
        }

        internal async Task DisableBreakpointAsync(LuaBreakpoint unboundBreakpoint, CancellationToken ct)
        {
            //if (HasExited)
            //{
            //    return;
            //}
            //SendRequest("DISABLE breakpoint\n");
        }

        #endregion
    }
}
