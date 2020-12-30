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
        private static ClientManager clientManager;
        private static Options options;

        public static Action<ConnectionPocket, Socket, int> OnConnection;
        public static Action<ChatMessagePocket, int> OnChatMessage;
        public static Action<DisconnectionPocket, int> onClientDisconnect;
        public static Action<PingPocket, int> onPingRecieved;
        public static Action<GameActionPocket, int> onGameAction;
        public static Action<PlayStatePocket, int> onPlayState;
        public static Action<int> OnMessageAccepted;

        private delegate BasePocket DeselialiseBytesToCommand(byte[] bytes);
        private static readonly Dictionary<PocketEnum, DeselialiseBytesToCommand> BytesToTypes = new Dictionary<PocketEnum, DeselialiseBytesToCommand>();

        static PocketHandler()
        {
            FillBytesToCommandsDictionary();
        }

        public static void Init(ClientManager clientManager, Options _options)
        {
            PocketHandler.clientManager = clientManager;
            options = _options;
        }

        private static void FillBytesToCommandsDictionary()
        {
            BytesToTypes.Add(PocketEnum.Connection, ConnectionPocket.FromBytes);
            BytesToTypes.Add(PocketEnum.ChatMessage, ChatMessagePocket.FromBytes);
            BytesToTypes.Add(PocketEnum.Disconnection, DisconnectionPocket.FromBytes);
            BytesToTypes.Add(PocketEnum.Ping, PingPocket.FromBytes);
            BytesToTypes.Add(PocketEnum.GameAction, GameActionPocket.FromBytes);
            BytesToTypes.Add(PocketEnum.PlayState, PlayStatePocket.FromBytes);
        }

        public static void HandleClientMessage(Socket client)
        {
            int client_id = -1;
            byte[] rest_data = null;
            do
            {
                try
                {
                    byte[] buffer = new byte[40000];
                    int size = client.Receive(buffer);
                    if (size > 0)
                    {
                        byte[] data = new byte[size];
                        Buffer.BlockCopy(buffer, 0, data, 0, size);
                        if (rest_data != null)
                            data = Utils.ConcatBytes(rest_data, data);
                        data = Encryption.Decrypt(data);
                        if (client_id > -1)
                            clientManager.SetRecieve(client_id, data);
                        rest_data = ParsePocket(data, client, ref client_id);
                    }
                }
                catch (Exception exception)
                {
                    DataManager.LogLine($"[ERROR]: {exception.Message} {exception.InnerException}", 2);
                    //if (client.Connected)
                        //client.Send((new ErrorPocket(2, exception.Message).ToBytes()));
                    rest_data = null;
                }
            } while (client.Connected);
            if (clientManager.GetClientName(client_id) != "unknown" && PocketListener.continueListen)
            {
                DataManager.LogLine($"[SERVER]: Lost connection with '{clientManager.GetClientName(client_id)}'");
                clientManager.ToggleConnectionState(client_id);
            }
            client.Shutdown(SocketShutdown.Both);
            client.Close();
        }

        private static byte[] ParsePocket(byte[] data, Socket client, ref int client_id)
        {
            do
            {
                bool accept = false;
                Header header = Header.FromBytes(data);
                if (clientManager.GetLastPocketID(client_id) == header.Id)
                    return Utils.SplitBytes(ref data, header.Size + Header.GetLenght());
                if (data.Length < Header.GetLenght() || data.Length < Header.GetLenght() + header.Size)
                    return null;
                Utils.SplitBytes(ref data, Header.GetLenght());
                var typeEnum = (PocketEnum)header.Type;
                if (typeEnum == PocketEnum.MessageAccepted)
                    OnMessageAccepted?.Invoke(client_id);
                else if (typeEnum == PocketEnum.Connection)
                {
                    ConnectionPocket pocket = ConnectionPocket.FromBytes(data);
                    int rec_id = (int)clientManager.FindClient(pocket.Name);
                    if (clientManager.GetClientState(rec_id) >= ClientStateEnum.Connected)
                    {
                        clientManager.SendRaw(client, new ConnectionPocket("Server", "This client is already connected to server"));
                        DataManager.LogLine($"[SERVER]: '{pocket.Name}' refused to connect due to same already exist client on server");
                    }
                    else
                    {
                        client_id = clientManager.GetAvailibleID();
                        if (rec_id > -1)
                            client_id = rec_id;
                        OnConnection?.Invoke(pocket, client, client_id);
                    }
                }
                else if (client_id > -1)
                {
                    accept = true;
                    if (typeEnum >= PocketEnum.SplittedPocketStart && typeEnum <= PocketEnum.SplittedPocketEnd)
                    {
                        if (header.Type == (int)PocketEnum.SplittedPocketStart)
                            clientManager.SetBuffer(client_id, null);
                        clientManager.AddBuffer(client_id, Utils.SplitBytes(data, header.Size));
                        if (header.Type == (int)PocketEnum.SplittedPocketEnd)
                        {
                            ParsePocket(clientManager.GetBuffer(client_id), client, ref client_id);
                            clientManager.SetBuffer(client_id, null);
                        }
                    }
                    else if (client_id < clientManager.GetMaxID() + 1)
                    {
                        BasePocket basePocket = BytesToTypes[typeEnum].Invoke(data);
                        switch (typeEnum)
                        {
                            case PocketEnum.ChatMessage:
                                OnChatMessage?.Invoke((ChatMessagePocket)basePocket, client_id);
                                break;
                            case PocketEnum.Disconnection:
                                onClientDisconnect?.Invoke((DisconnectionPocket)basePocket, client_id);
                                break;
                            case PocketEnum.Ping:
                                onPingRecieved?.Invoke((PingPocket)basePocket, client_id);
                                break;
                            case PocketEnum.GameAction:
                                onGameAction?.Invoke((GameActionPocket)basePocket, client_id);
                                break;
                            case PocketEnum.PlayState:
                                onPlayState?.Invoke((PlayStatePocket)basePocket, client_id);
                                break;
                            default:
                                DataManager.LogLine("Wrong pocket");
                                break;
                        }
                    }
                }
                Utils.SplitBytes(ref data, header.Size);
                if (accept)
                    clientManager.SendAccepted(client_id, header.Id);
            } while (data.Length >= Header.GetLenght());
            return data;
        }
    }
}
