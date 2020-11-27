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
        public static void SendSplittedPocket(byte[] data)
        {
            data = Utils.ConcatBytes(new MainHeader(332, (int)DateTime.Now.Ticks).ToBytes(), data);
            int split_count = (data.Length / 20 + 1) + 1000;
            do
            {
                byte[] pocket = data;
                if (data.Length > 20)
                    pocket = Utils.SplitBytes(ref data, 20);
                pocket = Utils.ConcatBytes(new Header(PocketEnum.SplittedPocket, split_count).ToBytes(), pocket);
                SendToServer(server, pocket);
                Thread.Sleep(100);
                if (split_count > 1000)
                    split_count %= 1000;
                split_count--;
            } while (split_count > 0);
        }

        public static void SendToServer(Socket server, byte[] data)
        {
            byte[] header = new MainHeader(332, (int)DateTime.Now.Ticks).ToBytes();
            data = Utils.ConcatBytes(header, data);
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
            SendToServer(server, PingPocket.ConstructSingle(pocket.Tick, pocket.LastPing));
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
                    Header header = new Header(PocketEnum.Disconnection, 1);
                    DisconnectionPocket pocket = new DisconnectionPocket(clientName, "Exit");
                    data = Utils.ConcatBytes(header, pocket);
                    SendToServer(server, data);
                }
                else if (message == "connect")
                {
                    server = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    server.ReceiveTimeout = 100;
                    server.Connect(ipEndPoint);
                    if (_listenThread != null)
                        _listenThread.Interrupt();
                    data = ConnectionPocket.ConstructSingle(clientName, "Connect");
                    _listenThread = new Thread(ListenForPockets);
                    _listenThread.Start(server);
                    SendToServer(server, data);
                }
                else if (message != "exit")
                {
                    Header header = new Header(PocketEnum.ChatMessage, 1);
                    ChatMessagePocket pocket = new ChatMessagePocket(clientName, message);
                    data = Utils.ConcatBytes(header, pocket);
                    (new Task(() => SendSplittedPocket(data))).Start();
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