using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Server.Pockets;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;
using Server.Enums;

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

            Settings _settings = new Settings();
            _clientManager.SetSettings(_settings);
            PocketListener pocketListener = new PocketListener(_clientManager, _settings);
            PocketManager.Init(_clientManager, _settings);
            PocketHandler.Init(_clientManager, _settings);
            Console.WriteLine("[INFO]:  Server is starting...");
            try
            {
                pocketListener.Start();
            }
            catch (Exception exception)
            {
                if (_settings.ExceptionPrint)
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
                        //Console.WriteLine("[SERVER] ---> [All Clients]: [Message]: {0}", str.StringField);
                        PocketManager.SendDataToAll(data);
                    }
                    else if (cmd == "list")
                    {
                        Console.WriteLine("   List of all connected clients");
                        for (int i = 0; i < _clientManager.ID_list.Count; i++)
                            Console.WriteLine("   " + _clientManager.GetClientInfo(_clientManager.ID_list[i]));
                    }
                    Thread.Sleep(100);
                }
            }

            static void PocketListener_OnAccept(int id)
            {
                Console.WriteLine("[SERVER] <--- [Accepted] from [Client]: {0}", _clientManager.GetClientName(id));
                _clientManager.UpdateAcceptState(id, true);
            }

            static void PocketListener_OnConnect(ConnectionPocket pocket, Socket client, int id)
            {
                if (id > -1 && id < _clientManager.GetAvailibleID())
                {
                    _clientManager.ReplaceClient(client, id);
                    Console.WriteLine("[SERVER]: '{0}' reconnected", pocket.Name, pocket.Message);
                }
                else
                {
                    _clientManager.AddClient(client, pocket.Name);
                    Console.WriteLine("[SERVER]: '{0}' connected", pocket.Name, pocket.Message);
                }
            }

            static void PocketListener_OnDisconnect(int id)
            {
                Console.WriteLine("[SERVER]: '{0}' disconnected", _clientManager.GetClientName(id));
                _clientManager.DeleteClient(id);
            }

            static void ClientManager_OnLostConnection(int id)
            {
                Console.WriteLine("[SERVER]: '{0}' disconnected (Timed out)", _clientManager.GetClientName(id));
                _clientManager.DeleteClient(id);
            }

            static void PocketListener_OnString(StringPocket pocket, int id)
            {
                Console.WriteLine("[SERVER] <--- [Client]: {0} [Message]: {1}", _clientManager.GetClientName(id), pocket.StringField);

                /// Resend example (like chat message)

                byte[] data = ChatMessagePocket.Construct(_clientManager.GetClientName(id), pocket.StringField);
                PocketManager.SendDataToAllExcept(data, id);
            }
        }

    }
}