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

        public int Id => _pid;

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
            processInfo.Arguments = @"C:\Users\Zhiyuan\Documents\NPL_Projects\NPLTools\NPL\visualstudio_lua_launcher.lua test.lua " + listenerPort;

            _process = new Process();
            _process.StartInfo = processInfo;
            _process.EnableRaisingEvents = true;
        }

        private static void AcceptConnection(IAsyncResult iar)
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
            // Create the state object.  
            StateObject state = new StateObject();
            state.workSocket = socket;
            socket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
            
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket.   
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.  
                state.sb.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read   
                // more data.  
                content = state.sb.ToString();
                if (content.IndexOf("<EOF>") > -1)
                {
                    // All the data has been read from the   
                    // client. Display it on the console.  
                    Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                        content.Length, content);
                    // Echo the data back to the client.  
                    Send(handler, content);
                }
                else
                {
                    // Not all data received. Get more.  
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
            }
        }

        private static void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.  
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
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


    }
}
