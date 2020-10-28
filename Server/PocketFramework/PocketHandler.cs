using Server.Enums;
using Server.Pockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    class PocketHandler
    {
        private static ClientManager _clientManager;

        public static Action<ConnectionPocket, Socket, int> OnConnectionPocket;
        public static Action<StringPocket, int> OnStringPocket;
        public static Action<ChatMessagePocket, int> OnChatMessagePocket;
        public static Action<int> onClientDisconnect;
        public static Action<int> OnMessageAccepted;

        private delegate BasePocket DeselialiseBytesToCommand(byte[] bytes);
        private static readonly Dictionary<PocketEnum, DeselialiseBytesToCommand> BytesToPockets = new Dictionary<PocketEnum, DeselialiseBytesToCommand>();

        static PocketHandler()
        {
            FillBytesToCommandsDictionary();
        }

        public static void SetClientManager(ClientManager clientManager)
        {
            _clientManager = clientManager;
        }

        private static void FillBytesToCommandsDictionary()
        {
            BytesToPockets.Add(PocketEnum.Connection, ConnectionPocket.FromBytes);
            BytesToPockets.Add(PocketEnum.String, StringPocket.FromBytes);
            BytesToPockets.Add(PocketEnum.ChatMessage, ChatMessagePocket.FromBytes);
        }

        public static void HandleClientMessage(Socket client, int client_id)
        {
            do
            {
                try
                {
                    byte[] buffer = new byte[1024];
                    int size = 0;
                    if (_clientManager.FindClient(client) == -1)
                        size = client.Receive(buffer);
                    else
                        size = _clientManager.Recieve(client_id, ref buffer);
                    byte[] data = new byte[size];
                    Buffer.BlockCopy(buffer, 0, data, 0, size);
                    ParsePocket(data, client, client_id);
                }
                catch (Exception exception)
                {
                    Console.WriteLine("[ERROR]: " + exception.Message + " " + exception.InnerException);
                }
            } while (client.Connected);
            //onClientDisconnect?.Invoke(client_id);
            _clientManager.SetAcceptState(client_id, false);
            client.Shutdown(SocketShutdown.Both);
            client.Close();
        }

        private static void ParsePocket(byte[] data, Socket client, int client_id)
        {
            if (data.Length >= HeaderPocket.GetLenght())
            {
                int skip_size = 0;
                bool accept = false;
                while (data.Length > skip_size)
                {
                    IEnumerable<byte> nextPocketBytes = data.Skip(skip_size);
                    HeaderPocket header = HeaderPocket.FromBytes(nextPocketBytes.ToArray());
                    skip_size += HeaderPocket.GetLenght();
                    for (int i = 0; i < header.Count; i++)
                    {
                        nextPocketBytes = data.Skip(skip_size);
                        var typeEnum = (PocketEnum)header.Type;
                        if (typeEnum == PocketEnum.MessageAccepted)
                            OnMessageAccepted?.Invoke(client_id);
                        else
                        {
                            BasePocket basePocket = BytesToPockets[typeEnum].Invoke(nextPocketBytes.ToArray());
                            skip_size += basePocket.ToBytes().Length;
                            switch (typeEnum)
                            {
                                case PocketEnum.Connection:
                                    OnConnectionPocket?.Invoke((ConnectionPocket)basePocket, client, client_id);
                                    break;
                                case PocketEnum.String:
                                    OnStringPocket?.Invoke((StringPocket)basePocket, client_id);
                                    break;
                                case PocketEnum.ChatMessage:
                                    break;
                            }
                            accept = true;
                        }
                    }
                }
                if (accept)
                    PocketSender.SendAcceptedToClient(client_id);
            }
        }
    }
}
