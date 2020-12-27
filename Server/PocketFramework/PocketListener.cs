using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    class PocketListener
    {
        Socket _listener;
        readonly Options options;
        IPEndPoint _ipEndpoint;
        Thread _listenThread;
        public static bool continueListen = true;

        public PocketListener(Options _options)
        {
            options = _options;
            IPHostEntry ipHost = Dns.GetHostEntry(options.HostName);
            IPAddress ipAddr = ipHost.AddressList[0];
            _ipEndpoint = new IPEndPoint(ipAddr, options.Port);
            _listener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _listener.Bind(_ipEndpoint);
            _listener.Listen(10);
        }

        public void Start()
        {
            _listenThread = new Thread(ListenForClients);
            _listenThread.Start();
        }

        private void ListenForClients()
        { 
            while (continueListen)
            {
                try
                {
                    Socket handler = _listener.Accept();
                    handler.LingerState = new LingerOption(true, 1);
                    var clientThread = new Thread(HandleClientPocket);
                    clientThread.Start(handler);
                }
                catch (Exception exception)
                {
                    DataManager.LogLine($"[ERROR]:  {exception.Message } - {exception.InnerException}", 2);
                }
            }
            _listener.Close();
        }

        private void HandleClientPocket(object client)
        {
            PocketHandler.HandleClientMessage((Socket)client);
        }

        public void Stop()
        {
            continueListen = false;
            _listener.Close();
            _listenThread.Interrupt();
        }
    }
}
