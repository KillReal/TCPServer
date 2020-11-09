﻿using Server.Enums;
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

        public static Action<ConnectionPocket, Socket, int> OnConnection;
        public static Action<ChatMessagePocket, int> OnChatMessage;
        public static Action<DisconnectionPocket, int> onClientDisconnect;
        public static Action<PingPocket, int> onPingRecieved;
        public static Action<int> OnMessageAccepted;

        private delegate BasePocket DeselialiseBytesToCommand(byte[] bytes);
        private static readonly Dictionary<PocketEnum, DeselialiseBytesToCommand> BytesToTypes = new Dictionary<PocketEnum, DeselialiseBytesToCommand>();

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
            BytesToTypes.Add(PocketEnum.Connection, ConnectionPocket.FromBytes);
            BytesToTypes.Add(PocketEnum.ChatMessage, ChatMessagePocket.FromBytes);
            BytesToTypes.Add(PocketEnum.Disconnection, DisconnectionPocket.FromBytes);
            BytesToTypes.Add(PocketEnum.Ping, PingPocket.FromBytes);
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
                    if (_settings.EncryptionEnabled)
                        data = Encryption.Decrypt(data);
                    if (data == null)
                    {
                        if (_settings.ExceptionPrint)
                            Console.WriteLine("[ERROR]: Decryption failed");
                    }
                    else
                    {
                        if (client_id > -1)
                            _clientManager.SetRecieve(client_id, data);
                        new Task(() => ParsePocket(data, client, ref client_id)).Start();
                    }
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
                        else if (client_id == -1 && typeEnum == PocketEnum.Connection)
                        {
                            ConnectionPocket pocket = ConnectionPocket.FromBytes(nextPocketBytes.ToArray());
                            int rec_id = (int)_clientManager.FindClient(pocket.Name);
                            client_id = _clientManager.GetAvailibleID();
                            if (rec_id > -1 && _clientManager.GetClientState(rec_id) == (int)(ClientStateEnum.Disconnected))
                                client_id = rec_id;
                            OnConnection?.Invoke(pocket, client, client_id);
                            skip_size = data.Length;
                            break;
                        }
                        else if (client_id > -1 && client_id < _clientManager.GetMaxID() + 1 && skip_size != data.Length)
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
                                default:
                                    skip_size = data.Length;
                                    break;
                            }
                            accept = true;
                        }
                        else
                            break;
                    }
                }
                if (accept)
                    PocketManager.SendAccepted(client_id);
            }
        }
    }
}
