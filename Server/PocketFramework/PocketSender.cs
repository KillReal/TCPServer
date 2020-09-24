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
        private static ClientManager _clientManager;
        public static void SetClientManager(ClientManager clientManager)
        {
            _clientManager = clientManager;
        }
        public static void SendAcceptedToClient(int id)
        {
            try
            {
                var headerPocket = new HeaderPocket
                {
                    Count = 1,
                    Type = (int)PocketEnum.MessageAccepted,
                    NeedAccept = false
                };
                _clientManager.Send(id, headerPocket.ToBytes());
            }
            catch (SocketException exception)
            {
                Console.WriteLine("[ERROR]: " + exception.Message + " " + exception.InnerException);
            }
        }

        public static void SendPocketToClient(int id, BasePocket pocket, PocketEnum typeEnum)
        {
            try
            {
                var pocketHeader = new HeaderPocket
                {
                    Count = 1,
                    Type = (int)typeEnum,
                    NeedAccept = true
                };
                byte[] data = Utils.ConcatByteArrays(pocketHeader.ToBytes(), pocket.ToBytes());
                _clientManager.Send(id, data);
            }
            catch (SocketException exception)
            {
                Console.WriteLine("[ERROR]: " + exception.Message + " " + exception.InnerException);
            }
        }

        static public void SendPocketToAll(byte[] data, bool accept = false)
        {
            for (int i = 0; i < _clientManager.GetAvailibleID(); i++)
                _clientManager.Send(i, data);
        }

        static public void SendPocketToAllExcept(byte[] data, int excepted_id, bool accept = false)
        {
            for (int i = 0; i < _clientManager.GetAvailibleID() - 1; i++)
                if (i != excepted_id)
                    _clientManager.Send(i, data);
        }
    }
}
