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
    public class LuaProcess
    {
        private readonly Process _process;

        private int _pid;

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
            processInfo.Arguments = @"C:\Users\Zhiyuan\Documents\NPL_Projects\NPLTools\NPL\visualstudio_lua_launcher.lua test.lua " + listenerPort + " > result.txt";

            _process = new Process();
            _process.StartInfo = processInfo;
            _process.EnableRaisingEvents = true;
        }

        private static async void AcceptConnection(IAsyncResult iar)
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

            var stream = new NetworkStream(socket, ownsSocket: true);
            var connectedEvent = new AutoResetEvent(false);
            try
            {
                socket.Blocking = true;
                StreamReader reader = new StreamReader(stream);
                string line = await reader.ReadLineAsync().ConfigureAwait(false);
                connectedEvent.WaitOne(10000);
                //StreamWriter writer = new StreamWriter(stream);
                string str = "handshake from C#";
                var bytes = Encoding.UTF8.GetBytes(str);
                await stream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
            }
            catch (IOException)
            {
            }
            catch (Exception)
            {
            }
            finally
            {
                stream?.Dispose();
                socket?.Dispose();
            }
            socketSource.BeginAccept(AcceptConnection, socketSource);
        }

        public void Start()
        {
            try
            {
                bool success = _process.Start();
            }
            catch (Exception e)
            {

            }
        }


    }
}
