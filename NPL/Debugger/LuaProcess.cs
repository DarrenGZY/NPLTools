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
        private Socket _socketHandler;
        private NetworkStream _networkStream;
        private Thread _eventThread;
        private int _breakpointCounter;
        private readonly Dictionary<int, LuaBreakpoint> _breakpoints = new Dictionary<int, LuaBreakpoint>();
        public int Id => _pid;

        public event EventHandler<EventArgs> ModuleLoad;
        public event EventHandler<EventArgs> BreakPointHit;

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
            processInfo.WorkingDirectory = @"C:\Users\Zhiyuan\Documents\NPL_Projects\NPLTools\NPL";
            //processInfo.WorkingDirectory = @"C:\Users\Zhiyuan\Documents\Visual Studio 2015\Projects\NPL Express Framework24\NPL Express Framework24";
            processInfo.Arguments = @"C:\Users\Zhiyuan\Documents\NPL_Projects\NPLTools\NPL\visualstudio_lua_debugger.lua test.lua " + listenerPort;
            //processInfo.Arguments = @"C:\Users\Zhiyuan\Documents\NPL_Projects\NPLTools\NPL\test.lua";
            _process = new Process();
            _process.StartInfo = processInfo;
            _process.EnableRaisingEvents = true;
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

        public void EventHandlingThread()
        {
            while (true)
            {
                string res;
                if ((res = ReceiveRequest()) != String.Empty)
                {
                    if (res == "moduleload")
                        ModuleLoad?.Invoke(this, new EventArgs());
                    else if (res == "breakpointhit")
                        BreakPointHit?.Invoke(this, new EventArgs());
                }
            }
        }

        public string ReceiveRequest()
        {
            string received = String.Empty;
            StreamReader reader = new StreamReader(_networkStream);
            received = reader.ReadLine();
            return received;
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

        #region breakpoints related functions
        public LuaBreakpoint AddBreakpoint(
                                string filename,
                                int lineNo,
                                LuaBreakpointConditionKind conditionKind = LuaBreakpointConditionKind.Always,
                                string condition = "",
                                LuaBreakpointPassCountKind passCountKind = LuaBreakpointPassCountKind.Always,
                                int passCount = 0
                             )
        {
            int id = _breakpointCounter++;
            var res = new LuaBreakpoint(this, filename, lineNo, conditionKind, condition, passCountKind, passCount, id);
            _breakpoints[id] = res;
            return res;
        }

        internal async Task BindBreakpointAsync(LuaBreakpoint breakpoint, CancellationToken ct)
        {
            SendRequest("BIND breakpoint\n");
        }

        internal async Task SetBreakpointConditionAsync(LuaBreakpoint breakpoint, CancellationToken ct)
        {
            SendRequest("SET breakpoint condition\n");
        }

        internal async Task SetBreakpointPassCountAsync(LuaBreakpoint breakpoint, CancellationToken ct)
        {
            SendRequest("SET breakpoint passcount\n");
        }

        internal async Task SetBreakpointHitCountAsync(LuaBreakpoint breakpoint, int count, CancellationToken ct)
        {
            SendRequest("SET breakpoint hit count\n");
        }

        internal async Task<int> GetBreakpointHitCountAsync(LuaBreakpoint breakpoint, CancellationToken ct)
        {
            SendRequest("SET breakpoint hit count\n");
            return 1;
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
            SendRequest("DISABLE breakpoint\n");
        }

        #endregion
    }
}
