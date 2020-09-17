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
        public static Action<StringPocket, Socket> OnStringPocket;
        public static Action OnMessageAccepted;

        private delegate BasePocket DeselialiseBytesToCommand(byte[] bytes);
        private static readonly Dictionary<PocketEnum, DeselialiseBytesToCommand> BytesToCommands = new Dictionary<PocketEnum, DeselialiseBytesToCommand>();

        static PocketHandler()
        {
            FillBytesToCommandsDictionary();
        }

        private static void FillBytesToCommandsDictionary()
        {
            BytesToCommands.Add(PocketEnum.Connection, ConnectionPocket.FromBytes);
            BytesToCommands.Add(PocketEnum.String, StringPocket.FromBytes);
        }

        public static void HandleClientMessage(Socket client)
        {

            do
            {
                byte[] data = new byte[1024];
                int bytesRec = client.Receive(data);
                if (bytesRec > 0)
                {
                    Parse(data, client);
                }
            } while (client.Connected);
            client.Shutdown(SocketShutdown.Both);
            client.Close();
        }

        private static void Parse(byte[] bytes, Socket client)
        {
            if (bytes.Length >= HeaderPocket.GetLenght())
            {
                HeaderPocket pocketHeader = HeaderPocket.FromBytes(bytes);
                IEnumerable<byte> nextCommandBytes = bytes.Skip(HeaderPocket.GetLenght());

                var typeEnum = (PocketEnum)pocketHeader.Type;

                if (typeEnum == PocketEnum.MessageAccepted)
                {
                    if (OnMessageAccepted != null)
                        OnMessageAccepted();
                }
                else
                {
                    Pockets.BasePocket basePocket = BytesToCommands[typeEnum].Invoke(nextCommandBytes.ToArray());

                    switch (typeEnum)
                    {
                        case PocketEnum.Connection:
                            OnConnectionPocket?.Invoke((ConnectionPocket)basePocket, client);
                            break;
                        case PocketEnum.String:
                            OnStringPocket?.Invoke((StringPocket)basePocket, client);
                            break;
                    }
                }
            }
        }
    }
}
