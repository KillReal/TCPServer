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
        readonly Settings _settings;
        ClientManager _clientManager;
        IPEndPoint _ipEndpoint;
        Thread _listenThread;
        public static bool _continueListen = true;

        public PocketListener(ClientManager clientManager, Settings settings)
        {
            _settings = settings;
            _clientManager = clientManager;
            IPHostEntry ipHost = Dns.GetHostEntry(_settings.HostName);
            IPAddress ipAddr = ipHost.AddressList[0];
            _ipEndpoint = new IPEndPoint(ipAddr, _settings.Port);
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
            while (_continueListen)
            {
                try
                {
                    Socket handler = _listener.Accept();
                    handler.ReceiveTimeout = 100;
                    var clientThread = new Thread(HandleClientPocket);
                    clientThread.Start(handler);
                }
                catch (Exception exception)
                {
                    if (_settings.ExceptionPrint)
                        Console.WriteLine($"[ERROR]:  {exception.Message } - {exception.InnerException}");
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
            _continueListen = false;
            _listener.Close();
            _listenThread.Interrupt();
        }
    }
}
