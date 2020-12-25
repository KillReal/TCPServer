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
            byte[] rest_data = null;
            do
            {
                try
                {
                    byte[] buffer = new byte[1024];
                    int size = server.Receive(buffer);
                    if (size != 0)
                    {
                        byte[] data = new byte[size];
                        Buffer.BlockCopy(buffer, 0, data, 0, size);
                        if (rest_data != null)
                            data = Utils.ConcatBytes(rest_data, data);
                        data = Encryption.Decrypt(data);
                        rest_data = ParsePocket(data, server);
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

        private static byte[] ParsePocket(byte[] data, Socket server)
        {
            while (data.Length >= Header.GetLenght())
            {
                bool accept = false;
                Header header = Header.FromBytes(data);
                if (last_recieved_id == header.Id)
                    break;
                byte[] temp_data = data;
                Utils.SplitBytes(ref data, Header.GetLenght());
                var typeEnum = (PocketEnum)header.Type;
                if (typeEnum == PocketEnum.MessageAccepted)
                    OnMessageAccepted?.Invoke();
                else if (typeEnum == PocketEnum.Connection)
                {
                    ConnectionPocket pocket = ConnectionPocket.FromBytes(data);
                    OnConnectionPocket?.Invoke(pocket);
                }
                else
                {
                    accept = true;
                    if (typeEnum >= PocketEnum.SplittedPocketStart && typeEnum <= PocketEnum.SplittedPocketEnd)
                    {
                        if (header.Type == (int)PocketEnum.SplittedPocketStart)
                            buffer = null;
                        buffer = Utils.ConcatBytes(buffer, Utils.SplitBytes(data, header.Size));
                        if (header.Type == (int)PocketEnum.SplittedPocketEnd)
                        {
                            ParsePocket(buffer, server);
                            buffer = null;
                        }
                    }
                    else
                    {
                        BasePocket basePocket = BytesToTypes[typeEnum].Invoke(data);
                        switch (typeEnum)
                        {
                            case PocketEnum.ChatMessage:
                                OnChatMessagePocket?.Invoke((ChatMessagePocket)basePocket);
                                break;
                            case PocketEnum.Disconnection:
                                OnDisconnectionPocket?.Invoke((DisconnectionPocket)basePocket);
                                break;
                            case PocketEnum.Ping:
                                onPingPocket?.Invoke((PingPocket)basePocket);
                                break;
                            case PocketEnum.Error:
                                onPingPocket?.Invoke((PingPocket)basePocket);
                                break;
                            default:
                                break;
                        }
                    }
                }
                Utils.SplitBytes(ref data, header.Size);
                if (accept)
                {
                    (new Task(() => Client.Client.SendToServer(server, new AcceptPocket(last_recieved_id)))).Start();
                }
            }
            return data;
        }
    }
}
