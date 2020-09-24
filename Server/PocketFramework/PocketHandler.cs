using Server.Pockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

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
        private static readonly Dictionary<PocketEnum, DeselialiseBytesToCommand> BytesToCommands = new Dictionary<PocketEnum, DeselialiseBytesToCommand>();

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
            BytesToCommands.Add(PocketEnum.Connection, ConnectionPocket.FromBytes);
            BytesToCommands.Add(PocketEnum.String, StringPocket.FromBytes);
            BytesToCommands.Add(PocketEnum.ChatMessage, ChatMessagePocket.FromBytes);
        }

        public static void HandleClientMessage(Socket client, int client_id)
        {
            do
            {
                try
                {
                    byte[] data = new byte[1024];
                    if (_clientManager.FindClient(client) == -1)
                        client.Receive(data);
                    else
                        _clientManager.Recieve(client_id, ref data);
                    ParsePocket(data, client, client_id);
                }
                catch
                {

                }
            } while (client.Connected);
            onClientDisconnect?.Invoke(client_id);
            client.Shutdown(SocketShutdown.Both);
            client.Close();
        }

        private static void ParsePocket(byte[] data, Socket client, int client_id)
        {
            if (data.Length >= HeaderPocket.GetLenght())
            {
                HeaderPocket pocketHeader = HeaderPocket.FromBytes(data);
                IEnumerable<byte> nextCommandBytes = data.Skip(HeaderPocket.GetLenght());
                var typeEnum = (PocketEnum)pocketHeader.Type;
                bool needAccept = pocketHeader.NeedAccept;
                if (typeEnum == PocketEnum.MessageAccepted)
                    OnMessageAccepted?.Invoke(client_id);
                else
                {
                    BasePocket basePocket = BytesToCommands[typeEnum].Invoke(nextCommandBytes.ToArray());
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
                }
                if (needAccept)
                    PocketSender.SendAcceptedToClient(client_id);
                _clientManager.SetAcceptState(client_id, false);
            }
        }
    }
}
