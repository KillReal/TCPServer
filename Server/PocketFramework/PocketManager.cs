﻿using Server.Pockets;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Server
{
    public class PocketManager
    {
        private static ClientManager _clientManager;
        private static Settings _settings;
        public static void Init(ClientManager clientManager, Settings settings)
        {
            _settings = settings;
            _clientManager = clientManager;
        }
        
        public static void SendAccepted(int id)
        {
            try
            {
                var headerPocket = new Header(PocketEnum.MessageAccepted, 1);
                _clientManager.Send(id, headerPocket.ToBytes());
            }
            catch (Exception exception)
            {
                Console.WriteLine("[ERROR]:  " + exception.Message + " " + exception.InnerException);
            }
        }

        public static void SendSinglePocket(int id, BasePocket pocket, PocketEnum typeEnum)
        {
            try
            {
                var pocketHeader = new Header(typeEnum, 1);
                byte[] data = Utils.ConcatBytes(pocketHeader.ToBytes(), pocket.ToBytes());
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
                _clientManager.Send(i, data, true);
        }

        static public void SendDataToAllExcept(byte[] data, int excepted_id)
        {
            for (int i = 0; i < _clientManager.GetAvailibleID(); i++)
                if (i != excepted_id)
                    _clientManager.Send(i, data, true);
        }
    }
}
