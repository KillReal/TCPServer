using Server.Pockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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

        public static void HandleClientMessage(Socket server)
        {
            do
            {
                try
                {
                    byte[] buffer = new byte[1024];
                    int size = server.Receive(buffer);
                    byte[] data = new byte[size];
                    Buffer.BlockCopy(buffer, 0, data, 0, size);
                    new Task(() => ParsePocket(data, server)).Start();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex);
                }
            } while (server.Connected);
            onClientDisconnect?.Invoke();
            server.Shutdown(SocketShutdown.Both);
            server.Close();
        }

        private static void ParsePocket(byte[] data, Socket server)
        {
            if (data.Length >= MainHeader.GetLenght())
            {
                int skip_size = 0;
                bool accept = false;
                MainHeader mainHeader = MainHeader.FromBytes(data.ToArray());
                skip_size += MainHeader.GetLenght();
                while (data.Length > skip_size + HeaderPocket.GetLenght())
                {
                    IEnumerable<byte> nextPocketBytes = data.Skip(skip_size);
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
                                case PocketEnum.String:
                                    OnStringPocket?.Invoke((StringPocket)basePocket);
                                    break;
                                case PocketEnum.ChatMessage:
                                    OnChatMessagePocket?.Invoke((ChatMessagePocket)basePocket);
                                    break;
                            }
                            accept = true;
                        }
                    }
                }
                if (accept)
                {
                    byte[] accept_data = Utils.ConcatByteArrays(MainHeader.Construct(332, (int)DateTime.Now.Ticks), HeaderPocket.Construct((int)PocketEnum.MessageAccepted, 1));
                    server.Send(accept_data);
                }
            }
        }
    }
}
