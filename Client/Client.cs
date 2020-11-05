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
        static Thread _listenThread;
        static Thread _readThread;

        static void SendToServer(Socket server, BasePocket pocket, PocketEnum typeEnum)
        {
            var headerPocket = new HeaderPocket
            {
                Count = 1,
                Type = (int)typeEnum,
            };
            byte[] msg = Utils.ConcatByteArrays(headerPocket.ToBytes(), pocket.ToBytes());
            msg = Utils.ConcatByteArrays(MainHeader.Construct(332, (int)DateTime.Now.Ticks), msg);
            int bytesSent = server.Send(msg);
        }
        static void SendToServer(Socket server, byte[] data)
        {
            byte[] header = MainHeader.Construct(332, (int)DateTime.Now.Ticks);
            data = Utils.ConcatByteArrays(header, data);
            server.Send(data);
        }
        static void Main(string[] args)
        {
            PocketHandler.OnMessageAccepted += PocketListener_OnAcception;
            PocketHandler.OnStringPocket += PocketListener_OnString;
            PocketHandler.OnChatMessagePocket += PocketListener_OnChatMessage;

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

        private static void ListenForCommands(object server)
        {
            PocketHandler.HandleClientMessage((Socket)server);
        }

        private static void Reader(object Tserver)
        {
            Socket server = (Socket)Tserver;
            while (server.Connected)
            {
                if (Console.KeyAvailable)
                {
                    Console.Write("[CLIENT] ---> [Message]: ");
                    string message = Console.ReadLine();
                    HeaderPocket header = new HeaderPocket
                    {
                        Count = 2,
                        Type = (int)PocketEnum.String,
                    };
                    StringPocket pocket = new StringPocket
                    {
                        StringField = message
                    };
                    StringPocket pocket2 = new StringPocket
                    {
                        StringField = "testing double pocket"
                    };
                    byte[] data = Utils.ConcatByteArrays(header.ToBytes(), pocket.ToBytes());
                    data = Utils.ConcatByteArrays(data, pocket2.ToBytes());
                    SendToServer(server, data);
                }
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

            ConnectionPocket connectPocket = new ConnectionPocket
            {
                Message = "",
                Name = clientName
            };

            SendToServer(server, connectPocket, PocketEnum.MessageAccepted);
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
        private static void PocketListener_OnString(StringPocket pocket)
        {
            Console.WriteLine("[CLIENT] <--- [Message]: {0}", pocket.StringField);
        }

        private static void PocketListener_OnChatMessage(ChatMessagePocket pocket)
        {
            Console.WriteLine("[CLIENT] <--- [Client]: {0} [Message]: {1}", pocket.Name, pocket.Message);
        }

    }
}