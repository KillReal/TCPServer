﻿using Server.Pockets;
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
            if (data.Length >= MainHeader.GetLenght())
            {
                int skip_size = 0;
                bool accept = false;
                MainHeader mainHeader = MainHeader.FromBytes(data.ToArray());
                if (mainHeader.Id == last_recieved_id)
                    return;
                last_recieved_id = mainHeader.Id;
                skip_size += MainHeader.GetLenght();
                while (data.Length > skip_size + Header.GetLenght())
                {
                    IEnumerable<byte> nextPocketBytes = data.Skip(skip_size);
                    Header header = Header.FromBytes(nextPocketBytes.ToArray());
                    skip_size += Header.GetLenght();
                    for (int i = 0; i < header.Count; i++)
                    {
                        nextPocketBytes = data.Skip(skip_size);
                        var typeEnum = (PocketEnum)header.Type;
                        if (typeEnum == PocketEnum.MessageAccepted)
                            OnMessageAccepted?.Invoke();
                        else if (typeEnum == PocketEnum.SplittedPocket)
                        {
                            if (header.Count > 1000)
                            {
                                buffer = null;
                                header.Count %= 1000;
                            }
                            Utils.SplitBytes(ref data, Header.GetLenght() + MainHeader.GetLenght());
                            buffer = Utils.ConcatBytes(buffer, data);           
                            if (header.Count == 1)
                            {
                                ParsePocket(buffer, server);
                                buffer = null;
                                return;
                            }
                            accept = true;
                            break;
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
                                    byte[] accept_data = Utils.ConcatBytes(new MainHeader(332, (int)DateTime.Now.Ticks), new Header(PocketEnum.MessageAccepted, 1));
                                    accept_data = Encryption.Encrypt(accept_data);
                                    server.Send(accept_data);
                                    OnDisconnectionPocket?.Invoke((DisconnectionPocket)basePocket);
                                    return;
                            }
                        }
                        accept = true;
                    }
                }
                if (accept)
                {
                    Header header = new Header(PocketEnum.MessageAccepted, 1);
                    (new Task(() => Client.Client.SendToServer(server, header.ToBytes()))).Start();
                }
            }
        }
    }
}
