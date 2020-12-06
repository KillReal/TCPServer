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
        private static ClientManager clientManager;
        private static Settings _settings;

        public static Action<ConnectionPocket, Socket, int> OnConnection;
        public static Action<ChatMessagePocket, int> OnChatMessage;
        public static Action<DisconnectionPocket, int> onClientDisconnect;
        public static Action<PingPocket, int> onPingRecieved;
        public static Action<GameActionPocket, int> onGameAction;
        public static Action<int> OnMessageAccepted;

        private delegate BasePocket DeselialiseBytesToCommand(byte[] bytes);
        private static readonly Dictionary<PocketEnum, DeselialiseBytesToCommand> BytesToTypes = new Dictionary<PocketEnum, DeselialiseBytesToCommand>();

        static PocketHandler()
        {
            FillBytesToCommandsDictionary();
        }

        public static void Init(ClientManager clientManager, Settings settings)
        {
            PocketHandler.clientManager = clientManager;
            _settings = settings;
        }

        private static void FillBytesToCommandsDictionary()
        {
            BytesToTypes.Add(PocketEnum.Connection, ConnectionPocket.FromBytes);
            BytesToTypes.Add(PocketEnum.ChatMessage, ChatMessagePocket.FromBytes);
            BytesToTypes.Add(PocketEnum.Disconnection, DisconnectionPocket.FromBytes);
            BytesToTypes.Add(PocketEnum.Ping, PingPocket.FromBytes);
            BytesToTypes.Add(PocketEnum.GameAction, GameActionPocket.FromBytes);
        }

        public static void HandleClientMessage(Socket client)
        {
            int client_id = -1;
            do
            {
                try
                {
                    byte[] buffer = new byte[1024];
                    int size = client.Receive(buffer);
                    if (size != 0)
                    {
                        byte[] data = new byte[size];
                        Buffer.BlockCopy(buffer, 0, data, 0, size);
                        data = Encryption.Decrypt(data);
                        if (client_id > -1)
                            clientManager.SetRecieve(client_id, data);
                        new Task(() => ParsePocket(data, client, ref client_id)).Start();
                    }
                }
                catch (Exception exception)
                {
                    if (_settings.ExceptionPrint)
                        Console.WriteLine("[ERROR]: " + exception.Message + " " + exception.InnerException);
                }
            } while (client.Connected && PocketListener._continueListen);
            if (clientManager.GetClientName(client_id) != "unknown" && clientManager.GetClientName(client_id) != null)
            {
                Console.WriteLine("[SERVER]: Lost connection with '{0}'", clientManager.GetClientName(client_id));
                clientManager.ToggleConnectionState(client_id);
            }
            client.Shutdown(SocketShutdown.Both);
            client.Close();
        }

        private static void ParsePocket(byte[] data, Socket client, ref int client_id)
        {
            try
            {
                if (data.Length >= MainHeader.GetLenght())
                {
                    int skip_size = 0;
                    bool accept = false;
                    MainHeader mainHeader = MainHeader.FromBytes(data.ToArray());
                    if (mainHeader.Hash != _settings.PocketHash || clientManager.GetLastPocketID(client_id) == mainHeader.Id)
                        return;
                    skip_size += MainHeader.GetLenght();
                    while (data.Length >= skip_size + Header.GetLenght())
                    {
                        IEnumerable<byte> nextPocketBytes = data.Skip(skip_size);
                        Header header = Header.FromBytes(nextPocketBytes.ToArray());
                        skip_size += Header.GetLenght();
                        for (int i = 0; i < header.Count; i++)
                        {
                            var typeEnum = (PocketEnum)header.Type;
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
                                int rec_id = (int)clientManager.FindClient(pocket.Name);
                                client_id = clientManager.GetAvailibleID();
                                if (rec_id > -1)
                                    client_id = rec_id;
                                OnConnection?.Invoke(pocket, client, client_id);
                                skip_size = data.Length;
                                break;
                            }
                            else if (client_id > -1)
                            {
                                accept = true;
                                if (typeEnum == PocketEnum.SplittedPocket)
                                {
                                    if (header.Count > 1000)
                                    {
                                        clientManager.SetBuffer(client_id, null);
                                        header.Count %= 1000;
                                    }
                                    Utils.SplitBytes(ref data, Header.GetLenght() + MainHeader.GetLenght());
                                    clientManager.AddBuffer(client_id, data); 
                                    if (header.Count == 1)
                                    {
                                        ParsePocket(clientManager.GetBuffer(client_id), client, ref client_id);
                                        clientManager.SetBuffer(client_id, null);
                                        return;
                                    }
                                    break;
                                }
                                else if (client_id < clientManager.GetMaxID() + 1 && skip_size != data.Length)
                                {
                                    BasePocket basePocket = BytesToTypes[typeEnum].Invoke(nextPocketBytes.ToArray());
                                    skip_size += basePocket.ToBytes().Length;
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
                                        default:
                                            skip_size = data.Length;
                                            break;
                                    }
                                }
                            }
                            else
                                break;
                        }
                    }
                    if (accept)
                        clientManager.SendAccepted(client_id);
                }
            }
            catch (Exception exception)
            {
                //if (_settings.ExceptionPrint)
                    Console.WriteLine("[ERROR]:  " + exception.Message + " " + exception.InnerException);
            }
        }
    }
}
