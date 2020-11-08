using Server.Enums;
using Server.Pockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class PocketHandler
    {
        private static ClientManager _clientManager;
        private static Settings _settings;

        public static Action<ConnectionPocket, Socket, int> OnConnectionPocket;
        public static Action<ChatMessagePocket, int> OnChatMessagePocket;
        public static Action<int> onClientDisconnect;
        public static Action<int> OnMessageAccepted;

        private delegate BasePocket DeselialiseBytesToCommand(byte[] bytes);
        private static readonly Dictionary<PocketEnum, DeselialiseBytesToCommand> BytesToPockets = new Dictionary<PocketEnum, DeselialiseBytesToCommand>();

        static PocketHandler()
        {
            FillBytesToCommandsDictionary();
        }

        public static void Init(ClientManager clientManager, Settings settings)
        {
            _clientManager = clientManager;
            _settings = settings;
        }

        private static void FillBytesToCommandsDictionary()
        {
            BytesToPockets.Add(PocketEnum.Connection, ConnectionPocket.FromBytes);
            BytesToPockets.Add(PocketEnum.ChatMessage, ChatMessagePocket.FromBytes);
        }

        public static void HandleClientMessage(Socket client)
        {
            int client_id = -1;
            do
            {
                try
                {
                    byte[] buffer = new byte[1024];
                    int size = 0;
                    size = client.Receive(buffer);
                    byte[] data = new byte[size];
                    Buffer.BlockCopy(buffer, 0, data, 0, size);
                    if (client_id > -1)
                        _clientManager.SetRecieve(client_id, data);
                    new Task(() => ParsePocket(data, client, ref client_id)).Start();
                }
                catch (Exception exception)
                {
                    if (_settings.ExceptionPrint)
                        Console.WriteLine("[ERROR]: " + exception.Message + " " + exception.InnerException);
                }
            } while (client.Connected);
            Console.WriteLine("[SERVER]: Lost connection with '{0}'", _clientManager.GetClientName(client_id));
            _clientManager.ToggleConnectionState(client_id);
            client.Shutdown(SocketShutdown.Both);
            client.Close();
        }

        private static void ParsePocket(byte[] data, Socket client, ref int client_id)
        {
            if (data.Length >= MainHeader.GetLenght())
            {
                int skip_size = 0;
                bool accept = false;
                MainHeader mainHeader = MainHeader.FromBytes(data.ToArray());
                if (mainHeader.Hash != _settings.PocketHash || _clientManager.GetLastBufferID(client_id) == mainHeader.Id)
                    return;
                skip_size += MainHeader.GetLenght();
                while (data.Length >= skip_size + HeaderPocket.GetLenght())
                {
                    IEnumerable<byte> nextPocketBytes = data.Skip(skip_size);
                    HeaderPocket header = HeaderPocket.FromBytes(nextPocketBytes.ToArray());
                    skip_size += HeaderPocket.GetLenght();
                    for (int i = 0; i < header.Count; i++)
                    {
                        var typeEnum = (PocketEnum)header.Type;
                        if (typeEnum < 0 || (int)typeEnum > BytesToPockets.Count() + 1)
                            break;
                        if (skip_size != data.Length)
                            nextPocketBytes = data.Skip(skip_size);
                        if (typeEnum == PocketEnum.MessageAccepted)
                        {
                            OnMessageAccepted?.Invoke(client_id);
                            skip_size = data.Length;
                            break;
                        }
                        else if (typeEnum == PocketEnum.Connection)
                        {
                            ConnectionPocket pocket = ConnectionPocket.FromBytes(nextPocketBytes.ToArray());
                            int rec_id = (int)_clientManager.FindClient(pocket.Name);
                            client_id = _clientManager.GetAvailibleID();
                            if (rec_id > -1 && _clientManager.GetClientState(rec_id) == (int)(ClientStateEnum.Disconnected))
                                client_id = rec_id;
                            OnConnectionPocket?.Invoke(pocket, client, client_id);
                            skip_size = data.Length;
                            break;
                        }
                        else if (client_id > -1 && client_id < _clientManager.GetAvailibleID() && skip_size != data.Length)
                        {
                            BasePocket basePocket = BytesToPockets[typeEnum].Invoke(nextPocketBytes.ToArray());
                            skip_size += basePocket.ToBytes().Length;
                            switch (typeEnum)
                            {
                                case PocketEnum.ChatMessage:
                                    OnChatMessagePocket?.Invoke((ChatMessagePocket)basePocket, client_id);
                                    break;
                            }
                            accept = true;
                        }
                        else
                            break;
                    }
                }
                if (accept)
                    PocketManager.SendAcceptedToClient(client_id);
            }
        }
    }
}
