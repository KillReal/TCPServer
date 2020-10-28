using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Server.Pockets;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;

namespace Server
{
    class Program
    {
        static ClientManager _clientManager = new ClientManager();
        static void Main(string[] args)
        {
            PocketHandler.OnMessageAccepted += PocketListener_OnAccept;
            PocketHandler.OnConnectionPocket += PocketListener_OnConnect;
            PocketHandler.OnStringPocket += PocketListener_OnString;
            PocketHandler.onClientDisconnect += PocketListener_OnDisconnect;
            ClientManager.onClientLostConnection += ClientManager_OnLostConnection;

            Settings settings = new Settings();
            PocketListener pocketListener = new PocketListener(_clientManager, settings);
            PocketSender.SetClientManager(_clientManager);
            PocketHandler.SetClientManager(_clientManager);
            Console.WriteLine("[INFO]:  Server is starting...");
            try
            {
                pocketListener.Start();
            }
            catch (Exception exception)
            {
                Console.WriteLine("[ERROR]:  " + exception.Message + " " + exception.InnerException);
            }
            finally
            {
                Console.WriteLine("[INFO]:  Server started successfully! (Press <Q> for stop server)");
                while (true)
                {
                    string cmd = Console.ReadLine();
                    if (cmd == "quit")
                    {
                        Console.WriteLine("[INFO]:  Server is stopping...");
                        pocketListener.Stop();
                        Thread.Sleep(100);
                        Console.WriteLine("[INFO]:  Server is successfully stopped! Press any key to exit.");
                        Console.ReadKey();
                        Environment.Exit(0);
                    }
                    else if (cmd == "punch")
                    {
                        HeaderPocket header = new HeaderPocket
                        {
                            Count = 1,
                            Type = (int)PocketEnum.String,
                        };
                        StringPocket str = new StringPocket
                        {
                            StringField = "test"
                        };
                        byte[] data = Utils.ConcatByteArrays(header.ToBytes(), str.ToBytes());
                        Console.WriteLine("[SERVER] ---> [All Clients]: [Message]: {0}", str.StringField);
                        PocketSender.SendPocketToAll(data);
                    }
                    else if (cmd == "list")
                    {
                        Console.WriteLine("   List of all connected clients");
                        for (int i = 0; i < _clientManager.GetAvailibleID(); i++)
                            Console.WriteLine("   " + _clientManager.GetClientInfo(i));
                        Console.WriteLine("   ");
                    }
                    Thread.Sleep(100);
                }
            }

            static void PocketListener_OnAccept(int id)
            {
                //Console.WriteLine("[SERVER] <--- [Accepted] from [Client]: {0}", _clientManager.GetClientName(id));
                _clientManager.SetAcceptState(id, true);
            }

            static void PocketListener_OnConnect(ConnectionPocket pocket, Socket client, int id)
            {
                Console.WriteLine("[SERVER] <--- [Client]: {0} connected", pocket.Name, pocket.Message);
                _clientManager.AddClient(client, pocket.Name);
            }

            static void PocketListener_OnDisconnect(int id)
            {
                Console.WriteLine("[SERVER] <--- [Client]: {0} disconnected", _clientManager.GetClientName(id));
                _clientManager.DeleteClient(id);
            }

            static void ClientManager_OnLostConnection(int id)
            {
                Console.WriteLine("[SERVER] <--- Lost connection with {0}", _clientManager.GetClientName(id));
                _clientManager.DeleteClient(id);
            }

            static void PocketListener_OnString(StringPocket pocket, int id)
            {
                Console.WriteLine("[SERVER] <--- [Client]: {0} [Message]: {1}", _clientManager.GetClientName(id), pocket.StringField);

                /// Resend example (like chat message)

                byte[] data = ChatMessagePocket.Construct(_clientManager.GetClientName(id), pocket.StringField);
                PocketSender.SendPocketToAllExcept(data, id, true);
            }
        }

    }
}