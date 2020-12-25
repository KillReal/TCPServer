using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Server.Pockets;
using Server;
using System.Threading;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Client
{
    class Client
    {
        static string clientName = "unnamed";
        static Socket server;
        static Thread _listenThread;
        static Thread _readThread;
        static IPEndPoint ipEndPoint;
        static IPAddress ipAddr;
        public static void SendSplittedPocket(BasePocket pocket)
        {
            byte[] data = Utils.ConcatBytes(new Header((int)DateTime.Now.Ticks, pocket.GetType(), pocket.ToBytes().Length), pocket);
            int split_count = (data.Length / 20 + 1);
            bool first = true;
            do
            {
                byte[] data_part = data;
                if (data.Length > 20)
                    data_part = Utils.SplitBytes(ref data, 20);
                int pocket_enum = (int)PocketEnum.SplittedPocket;
                if (first)
                {
                    pocket_enum = (int)PocketEnum.SplittedPocketStart;
                    first = false;
                }
                if (split_count == 1)
                    pocket_enum = (int)PocketEnum.SplittedPocketEnd;
                Header header = new Header((int)DateTime.Now.Ticks, pocket_enum, data_part.Length);
                data_part = Utils.ConcatBytes(header.ToBytes(), data_part);
                SendToServer(server, data_part);
                //Thread.Sleep(10);
                split_count--;
            } while (split_count > 0);
        }

        public static void SendToServer(Socket server, BasePocket pocket)
        {
            byte[] data = pocket.ToBytes();
            byte[] header = new Header((int)DateTime.Now.Ticks, pocket.GetType(), data.Length).ToBytes();
            data = Utils.ConcatBytes(header, data);
            data = Encryption.Encrypt(data);
            try
            {
                server.Send(data);
            }
            catch { }
        }
        public static void SendToServer(Socket server, byte[] data)
        {
            data = Encryption.Encrypt(data);
            try
            {
                server.Send(data);
            }
            catch { }
        }
        static void Main(string[] args)
        {
            PocketHandler.OnMessageAccepted += PocketListener_OnAcception;
            PocketHandler.OnChatMessagePocket += PocketListener_OnChatMessage;
            PocketHandler.onPingPocket += PocketListener_OnPingRecieved;
            PocketHandler.OnConnectionPocket += PocketListener_OnConnection;
            PocketHandler.OnDisconnectionPocket += PocketListener_OnDisconnection;

            Console.Write("Enter client name: ");
            clientName = Console.ReadLine();
            try
            {
                SendMessageFromSocket(11000);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                Console.ReadLine();
            }
        }

        private static void PocketListener_OnConnection(ConnectionPocket pocket)
        {
            Console.WriteLine("[CLIENT] <--- Connection ({0})", pocket.Message);
        }

        private static void PocketListener_OnDisconnection(DisconnectionPocket pocket)
        {
            Console.WriteLine("[CLIENT] <--- Disconnected ({0})", pocket.Message);
            server.Disconnect(false);
        }

        private static void PocketListener_OnPingRecieved(PingPocket pocket)
        {
            Thread.Sleep(50); //Simulate ping
            SendToServer(server, new PingPocket(pocket.Tick, pocket.LastPing));
        }

        private static void ListenForPockets(object server)
        {
            if (server != null)
                PocketHandler.HandleClientMessage((Socket)server);
        }

        static void SendMessageFromSocket(int port)
        {
            IPHostEntry ipHost = Dns.GetHostEntry("localhost");
            ipAddr = ipHost.AddressList[0];
            ipEndPoint = new IPEndPoint(ipAddr, port);
            var cts = new CancellationTokenSource();
            bool continue_work = true;
            while (continue_work)
            {
                string message = Console.ReadLine();
                byte[] data;
                if (message == "disconnect")
                {
                    SendToServer(server, new DisconnectionPocket(clientName, "Exit"));
                }
                else if (message == "connect")
                {
                    server = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    server.ReceiveTimeout = 100;
                    server.Connect(ipEndPoint);
                    if (_listenThread != null)
                        _listenThread.Interrupt();
                    ConnectionPocket pocket = new ConnectionPocket(clientName, "Connect");
                    _listenThread = new Thread(ListenForPockets);
                    _listenThread.Start(server);
                    SendToServer(server, pocket);
                }
                else if (message == "play")
                {
                    SendToServer(server, new PlayStatePocket(0, 3));
                }
                else if (message == "stop play")
                {
                    SendToServer(server, new PlayStatePocket(0, 5));    
                }
                else if (message != "exit")
                {
                    (new Task(() => SendSplittedPocket(new ChatMessagePocket(clientName, message)))).Start();
                }
                else
                    continue_work = false;
                Thread.Sleep(500);
            }
            _listenThread.Interrupt();
            _readThread.Interrupt();
            server.Shutdown(SocketShutdown.Both);
            server.Close();
        }

        private static void PocketListener_OnAcception()
        {
            Console.Write("[CLIENT] <--- [Accepted]\n");
        }

        private static void PocketListener_OnChatMessage(ChatMessagePocket pocket)
        {
            Console.WriteLine("[CLIENT] <--- [{0}]:  [Message]: {1}", pocket.Name, pocket.Message);
        }

    }
}