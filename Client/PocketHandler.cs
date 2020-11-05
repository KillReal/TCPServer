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

        public static Action<ConnectionPocket, Socket> OnConnectionPocket;
        public static Action<StringPocket> OnStringPocket;
        public static Action<ChatMessagePocket> OnChatMessagePocket;
        public static Action onClientDisconnect;
        public static Action OnMessageAccepted;

        private delegate BasePocket DeselialiseBytesToCommand(byte[] bytes);
        private static readonly Dictionary<PocketEnum, DeselialiseBytesToCommand> BytesToTypes = new Dictionary<PocketEnum, DeselialiseBytesToCommand>();

        static PocketHandler()
        {
            FillBytesToCommandsDictionary();
        }

        private static void FillBytesToCommandsDictionary()
        {
            BytesToTypes.Add(PocketEnum.Connection, ConnectionPocket.FromBytes);
            BytesToTypes.Add(PocketEnum.String, StringPocket.FromBytes);
            BytesToTypes.Add(PocketEnum.ChatMessage, ChatMessagePocket.FromBytes);
        }

        public static void HandleClientMessage(Socket client)
        {
            do
            {
                try
                {
                    byte[] data = new byte[1024];
                    client.Receive(data);
                    ParsePocket(data, client);
                }
                catch
                {

                }
            } while (client.Connected);
            onClientDisconnect?.Invoke();
            client.Shutdown(SocketShutdown.Both);
            client.Close();
        }

        private static void ParsePocket(byte[] data, Socket server)
        {
            if (data.Length >= MainHeader.GetLenght())
            {
                int skip_size = 0;
                bool accept = false;
                skip_size += MainHeader.GetLenght();
                IEnumerable<byte> nextPocketBytes = data.Skip(skip_size);
                MainHeader mainHeader = MainHeader.FromBytes(nextPocketBytes.ToArray());
                while (data.Length > skip_size)
                {
                    nextPocketBytes = data.Skip(skip_size);
                    HeaderPocket header = HeaderPocket.FromBytes(nextPocketBytes.ToArray());
                    skip_size += HeaderPocket.GetLenght();
                    for (int i = 0; i < header.Count; i++)
                    {
                        nextPocketBytes = data.Skip(skip_size);
                        var typeEnum = (PocketEnum)header.Type;
                        if (typeEnum == PocketEnum.MessageAccepted)
                            OnMessageAccepted?.Invoke();
                        else
                        {
                            BasePocket basePocket = BytesToTypes[typeEnum].Invoke(nextPocketBytes.ToArray());
                            skip_size += basePocket.ToBytes().Length;
                            switch (typeEnum)
                            {
                                case PocketEnum.Connection:
                                    OnConnectionPocket?.Invoke((ConnectionPocket)basePocket, server);
                                    break;
                                case PocketEnum.String:
                                    OnStringPocket?.Invoke((StringPocket)basePocket);
                                    break;
                                case PocketEnum.ChatMessage:
                                    break;
                            }
                            accept = true;
                        }
                    }
                }
                if (accept)
                {
                    var MainHeader = new MainHeader
                    {
                        Hash = 332,
                        Id = (int)DateTime.Now.Ticks
                    };
                    var headerPocket = new HeaderPocket
                    {
                        Count = 1,
                        Type = (int)PocketEnum.MessageAccepted,
                    };
                    server.Send(Utils.ConcatByteArrays(mainHeader.ToBytes(), headerPocket.ToBytes()));
                }
            }
        }
    }
}
