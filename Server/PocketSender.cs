using Server.Pockets;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Server
{
    public class PocketSender
    {
        readonly Settings _settings;
        PocketSender(Settings settings)
        {
            _settings = settings;
        }
        public static void SendAcceptedToClient(Socket client)
        {
            var pocketHeader = new HeaderPocket
            {
                Count = 1,
                Type = (int)PocketEnum.MessageAccepted
            };
            SendAnswerToClient(client, pocketHeader.ToBytes());
        }

        /*public static void SendCommandToServer(string serverIp, BasePocket pocket, PocketEnum typeEnum)
        {
            var pocketHeader = new PocketHeader
            {
                Count = 1,
                Type = (int)typeEnum
            };

            byte[] pocketBytes = Utils.ConcatByteArrays(pocketHeader.ToBytes(), pocket.ToBytes());
            SendCommandToServer(serverIp, _settings.Port, pocketBytes);
        }

        private static void SendCommandToServer(string ipAddress, int port, byte[] messageBytes)
        {
            var client = new Socket();
            try
            {
                client.Connect(ipAddress, port);
                byte[] messageBytesWithEof = Utils.AddCommandLength(messageBytes);
                NetworkStream networkStream = client.GetStream();
                networkStream.Write(messageBytesWithEof, 0, messageBytesWithEof.Length);
                PocketHandler.HandleClientMessage(client);
            }
            catch (SocketException exception)
            {
                Console.WriteLine("[ERROR]: " + exception.Message + " " + exception.InnerException);
            }
        }*/

        private static void SendAnswerToClient(Socket client, byte[] data)
        {
            try
            {
                string reply = "Message accepted";
                byte[] msg = Encoding.UTF8.GetBytes(reply);
                client.Send(msg);
            }
            catch (SocketException exception)
            {
                Console.WriteLine("[ERROR]: " + exception.Message + " " + exception.InnerException);
            }

        }
    }
}
