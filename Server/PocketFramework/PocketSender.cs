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
        public static void SendAcceptedToClient(Socket client)
        {
            try
            {
                var headerPocket = new HeaderPocket
                {
                    Count = 1,
                    Type = (int)PocketEnum.MessageAccepted
                };
                if (client != null)
                    client.Send(headerPocket.ToBytes());
            }
            catch (SocketException exception)
            {
                Console.WriteLine("[ERROR]: " + exception.Message + " " + exception.InnerException);
            }
        }

        public static void SendPocketToClient(Socket client, BasePocket pocket, PocketEnum typeEnum)
        {
            try
            {
                var pocketHeader = new HeaderPocket
                {
                    Count = 1,
                    Type = (int)typeEnum
                };
                byte[] data = Utils.ConcatByteArrays(pocketHeader.ToBytes(), pocket.ToBytes());
                if (client != null)
                    client.Send(data);
            }
            catch (SocketException exception)
            {
                Console.WriteLine("[ERROR]: " + exception.Message + " " + exception.InnerException);
            }
        }
    }
}
