using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Server.Pockets;
using Server;

namespace Client
{
    class Client
    {
        static string clientName = "unnamed";
        static void SendToServer(Socket server, BasePocket pocket, PocketEnum typeEnum)
        {
            var headerPocket = new HeaderPocket
            {
                Count = 1,
                Type = (int)typeEnum
            };

            byte[] msg = Utils.ConcatByteArrays(headerPocket.ToBytes(), pocket.ToBytes());
            int bytesSent = server.Send(msg);
        }
        static void Main(string[] args)
        {
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

        static void SendMessageFromSocket(int port)
        {
            // Буфер для входящих данных
            byte[] bytes = new byte[1024];

            // Соединяемся с удаленным устройством

            // Устанавливаем удаленную точку для сокета
            IPHostEntry ipHost = Dns.GetHostEntry("localhost");
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);

            Socket server = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            server.Connect(ipEndPoint);

            // Соединяем сокет с удаленной точкой

            Console.Write("Msg: ");
            string message = Console.ReadLine();

            Console.WriteLine("Connecting with {0} ...", server.RemoteEndPoint.ToString());

            ConnectionPocket connectPocket = new ConnectionPocket
            {
                Message = message,
                Name = clientName
            };

            SendToServer(server, connectPocket, PocketEnum.Connection);

            // Получаем ответ от сервера
            int bytesRec = server.Receive(bytes);

            Console.WriteLine("[SERVER]: {0}", Encoding.UTF8.GetString(bytes, 0, bytesRec));
            while (server.Connected)
            {
                Console.Write("Msg: ");
                message = Console.ReadLine();
                StringPocket pocket = new StringPocket
                {
                    StringField = message
                };
                SendToServer(server, pocket, PocketEnum.String);
                bytesRec = server.Receive(bytes);
                Console.WriteLine("[SERVER]: {0}", Encoding.UTF8.GetString(bytes, 0, bytesRec));
            }

            // Освобождаем сокет
            server.Shutdown(SocketShutdown.Both);
            server.Close();
        }
    }
}