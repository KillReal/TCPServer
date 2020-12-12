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
        public static Action<ConnectionPocket> OnConnectionPocket;
        public static Action<DisconnectionPocket> OnDisconnectionPocket;
        public static Action<ChatMessagePocket> OnChatMessagePocket;
        public static Action<PingPocket> onPingPocket;
        public static Action OnMessageAccepted;

        private static int last_recieved_id;
        private static byte[] buffer;

        private delegate BasePocket DeselialiseBytesToCommand(byte[] bytes);
        private static readonly Dictionary<PocketEnum, DeselialiseBytesToCommand> BytesToTypes = new Dictionary<PocketEnum, DeselialiseBytesToCommand>();

        static PocketHandler()
        {
            FillBytesToCommandsDictionary();
        }

        private static void FillBytesToCommandsDictionary()
        {
            BytesToTypes.Add(PocketEnum.Connection, ConnectionPocket.FromBytes);
            BytesToTypes.Add(PocketEnum.ChatMessage, ChatMessagePocket.FromBytes);
            BytesToTypes.Add(PocketEnum.Disconnection, DisconnectionPocket.FromBytes);
            BytesToTypes.Add(PocketEnum.Ping, PingPocket.FromBytes);
        }

        public static void HandleClientMessage(Socket server)
        {
            do
            {
                try
                {
                    byte[] buffer = new byte[1024];
                    int size = server.Receive(buffer);
                    //Console.WriteLine("Recieved: " + size + " bytes");
                    if (size != 0)
                    {
                        byte[] data = new byte[size];
                        Buffer.BlockCopy(buffer, 0, data, 0, size);
                        data = Encryption.Decrypt(data);
                        ParsePocket(data, server);
                    }
                }
                catch (Exception ex)
                {
                    //Console.WriteLine("Error: " + ex);
                }
            } while (server.Connected);
            server.Shutdown(SocketShutdown.Both);
            server.Close();
        }

        private static void ParsePocket(byte[] data, Socket server)
        {
            if (data.Length >= Header.GetLenght())
            {
                int skip_size = 0;
                bool accept = false;
                Header header = Header.FromBytes(data.ToArray());
                if (header.Id == last_recieved_id)
                    return;
                last_recieved_id = header.Id;
                skip_size += Header.GetLenght();
                if (data.Length > skip_size + Header.GetLenght())
                {
                    IEnumerable<byte> nextPocketBytes = data.Skip(skip_size);
                    var typeEnum = (PocketEnum)header.Type;
                    if (typeEnum == PocketEnum.MessageAccepted)
                        OnMessageAccepted?.Invoke();
                    else if (typeEnum >= PocketEnum.SplittedPocketStart && typeEnum <= PocketEnum.SplittedPocketEnd)
                    {
                        if (header.Type == (int)PocketEnum.SplittedPocketStart)
                            buffer = null;
                        Utils.SplitBytes(ref data, Header.GetLenght());
                        buffer = Utils.ConcatBytes(buffer, data);
                        if (header.Type == (int)PocketEnum.SplittedPocketEnd)
                        {
                            ParsePocket(buffer, server);
                            buffer = null;
                            return;
                        }
                        return;
                    }
                    else
                    {
                        BasePocket basePocket = BytesToTypes[typeEnum].Invoke(nextPocketBytes.ToArray());
                        skip_size += basePocket.ToBytes().Length;
                        switch (typeEnum)
                        {
                            case PocketEnum.ChatMessage:
                                OnChatMessagePocket?.Invoke((ChatMessagePocket)basePocket);
                                break;
                            case PocketEnum.Ping:
                                onPingPocket?.Invoke((PingPocket)basePocket);
                                return;
                            case PocketEnum.Connection:
                                OnConnectionPocket?.Invoke((ConnectionPocket)basePocket);
                                break;
                            case PocketEnum.Disconnection:
                                Client.Client.SendToServer(server, new AcceptPocket(header.Id));
                                OnDisconnectionPocket?.Invoke((DisconnectionPocket)basePocket);
                                return;
                        }
                    }
                    accept = true;
                }
                if (accept)
                {
                    (new Task(() => Client.Client.SendToServer(server, new AcceptPocket(last_recieved_id)))).Start();
                }
            }
        }
    }
}
