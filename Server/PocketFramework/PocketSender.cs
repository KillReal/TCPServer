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
                };
                _clientManager.Send(id, headerPocket.ToBytes());
            }
            catch (Exception exception)
            {
                Console.WriteLine("[ERROR]:  " + exception.Message + " " + exception.InnerException);
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
                };
                byte[] data = Utils.ConcatByteArrays(pocketHeader.ToBytes(), pocket.ToBytes());
                _clientManager.Send(id, data);
            }
            catch (Exception exception)
            {
                Console.WriteLine("[ERROR]:  " + exception.Message + " " + exception.InnerException);
            }
        }

        static public void SendDataToAll(byte[] data)
        {
            for (int i = 0; i < _clientManager.GetAvailibleID(); i++)
                _clientManager.Send(i, data);
        }

        static public void SendDataToAllExcept(byte[] data, int excepted_id)
        {
            for (int i = 0; i < _clientManager.GetAvailibleID(); i++)
                if (i != excepted_id)
                    _clientManager.Send(i, data);
        }
    }
}
