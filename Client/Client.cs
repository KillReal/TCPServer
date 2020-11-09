using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Server.Pockets;
using Server;
using System.Threading;
using System.Runtime.InteropServices;

namespace Client
{
    class Client
    {
        static string clientName = "unnamed";
        static Socket server;
        static Thread _listenThread;
        static Thread _readThread;

        static void SendToServer(Socket server, BasePocket pocket, PocketEnum typeEnum)
        {
            var headerPocket = new Header(typeEnum, 1);
            byte[] msg = Utils.ConcatBytes(headerPocket, pocket);
            msg = Utils.ConcatBytes(new MainHeader(332, (int)DateTime.Now.Ticks).ToBytes(), msg);
            msg = Encryption.Encrypt(msg);
            server.Send(msg);
        }
        static void SendToServer(Socket server, byte[] data)
        {
            byte[] header = new MainHeader(332, (int)DateTime.Now.Ticks).ToBytes();
            data = Utils.ConcatBytes(header, data);
            data = Encryption.Encrypt(data);
            server.Send(data);
        }
        static void Main(string[] args)
        {
            PocketHandler.OnMessageAccepted += PocketListener_OnAcception;
            PocketHandler.OnChatMessagePocket += PocketListener_OnChatMessage;
            PocketHandler.onPingPocket += PocketListener_OnPingRecieved;

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

        private static void PocketListener_OnPingRecieved(PingPocket pocket)
        {
            Thread.Sleep(50); //Simulate ping
            SendToServer(server, PingPocket.ConstructSingle(pocket.Tick, pocket.LastPing));
        }

        private static void ListenForCommands(object server)
        {
            PocketHandler.HandleClientMessage((Socket)server);
        }

        private static void Reader(object Tserver)
        {
            server = (Socket)Tserver;
            while (server.Connected)
            {
                string message = Console.ReadLine();
                byte[] data;
                if (message == "disconnect")
                {
                    Header header = new Header(PocketEnum.Disconnection, 1);
                    DisconnectionPocket pocket = new DisconnectionPocket(clientName, "exit");
                    data = Utils.ConcatBytes(header, pocket);
                }
                else
                {
                    Header header = new Header(PocketEnum.ChatMessage, 1);
                    ChatMessagePocket pocket = new ChatMessagePocket(clientName, message);
                    data = Utils.ConcatBytes(header, pocket);
                }
                SendToServer(server, data);
            }
        }

        static void SendMessageFromSocket(int port)
        {
            IPHostEntry ipHost = Dns.GetHostEntry("localhost");
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);
            Socket server = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            server.Connect(ipEndPoint);
            Console.WriteLine("[INFO]: Connecting with {0} ...", server.RemoteEndPoint.ToString());

            ConnectionPocket connectPocket = new ConnectionPocket(clientName, "connect");

            //SendToServer(server, connectPocket, PocketEnum.MessageAccepted);
            //Thread.Sleep(1000);
            SendToServer(server, connectPocket, PocketEnum.Connection);
            _listenThread = new Thread(ListenForCommands);
            _listenThread.Start(server);
            var cts = new CancellationTokenSource();
            _readThread = new Thread(Reader);
            _readThread.Start(server);
            while (server.Connected)
                Thread.Sleep(500);
            _listenThread.Interrupt();
            _readThread.Interrupt();
            server.Shutdown(SocketShutdown.Both);
            server.Close();
        }

        private static void PocketListener_OnAcception()
        {
            Console.Write("[ClIENT] <--- [Accepted]\n");
        }

        private static void PocketListener_OnChatMessage(ChatMessagePocket pocket)
        {
            Console.WriteLine("[CLIENT] <--- [Client]: {0} [Message]: {1}", pocket.Name, pocket.Message);
        }

    }
}