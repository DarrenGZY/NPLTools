using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
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

        private static void AcceptConnection(IAsyncResult iar)
        {

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
