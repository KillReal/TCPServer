using Server.Pockets;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

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

        public static void SendSplittedPocket(int id, byte[] data)
        {
            data = Utils.ConcatBytes(new MainHeader(_settings.PocketHash, (int)DateTime.Now.Ticks).ToBytes(), data);
            int split_count = data.Length / _settings.MaxPocketSize + 1;
            do
            {
                byte[] pocket = data;
                if (data.Length > _settings.MaxPocketSize)
                    pocket = Utils.SplitBytes(ref data, _settings.MaxPocketSize);
                pocket = Utils.ConcatBytes(new Header(PocketEnum.SplittedPocket, split_count).ToBytes(), pocket);
                _clientManager.Send(id, pocket, true);
                split_count--;
                Thread.Sleep(100);
            } while (_clientManager.GetSocket(id) != null && split_count > 0);
        }
        
        public static void SendAccepted(int id)
        {
            var header = new Header(PocketEnum.MessageAccepted, 1);
            _clientManager.Send(id, header.ToBytes());
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

        static public void SendDataToAll(byte[] data, bool require_accept = true)
        {
            for (int i = 0; i < _clientManager.GetAvailibleID(); i++)
            {
                if (data.Length > _settings.MaxPocketSize)
                    SendSplittedPocket(i, data);
                else
                    _clientManager.Send(i, data, require_accept);
            }
        }

        static public void SendDataToAllExcept(byte[] data, int excepted_id, bool require_accept = true)
        {
            for (int i = 0; i < _clientManager.GetAvailibleID(); i++)
            {
                if (i != excepted_id)
                {
                    if (data.Length > _settings.MaxPocketSize)
                        SendSplittedPocket(i, data);
                    else
                        _clientManager.Send(i, data, require_accept);
                }
            }
        }
    }
}
