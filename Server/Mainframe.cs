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
            PocketHandler.OnConnection += PocketListener_OnConnect;
            PocketHandler.OnChatMessage += PocketListener_OnChatMessage;
            PocketHandler.onClientDisconnect += PocketListener_OnDisconnect;
            ClientManager.onClientLostConnection += ClientManager_OnLostConnection;

            Settings _settings = new Settings();
            _clientManager.SetSettings(_settings);
            PocketListener pocketListener = new PocketListener(_clientManager, _settings);
            Encryption.Init(_settings);
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
                    Console.WriteLine($"[ERROR]:  {exception.Message } - {exception.InnerException}");
            }
            Console.WriteLine("[INFO]:  Server started successfully! (Type <exit> to console for stop server)");

            while (true)
            {
                switch (Console.ReadLine())
                {
                    case "quit":
                        Console.WriteLine("[INFO]:  Server is stopping...");
                        pocketListener.Stop();
                        Console.WriteLine("[INFO]:  Server is successfully stopped! Press any key to exit.");
                        Console.ReadKey();
                        return;

                    case "ping":
                        {
                            Header header = new Header(PocketEnum.ChatMessage, 1);
                            ChatMessagePocket str = new ChatMessagePocket("Server", "Test");
                            byte[] data = Utils.ConcatBytes(header, str);
                            Console.WriteLine($"[SERVER] ---> [All Clients]: [Message]: {str.Message}");
                            _clientManager.SendToAll(data);
                            break;
                        }

                    case "list":
                        {
                            Console.WriteLine("   List of all connected clients");
                            for (int i = 0; i < _clientManager.ID_list.Count; i++)
                                Console.WriteLine("   " + _clientManager.GetClientInfo(_clientManager.ID_list[i]));
                            break;
                        }

                    case "clr":
                        Console.Clear();
                        break;
                        
                    case "kick":
                        {
                            Console.Write("Enter client id: ");
                            int id = Convert.ToInt32(Console.ReadLine());
                            if (id > -1 && id < _clientManager.GetMaxID())
                            {
                                Console.WriteLine($"[SERVER]: Client '{_clientManager.GetClientName(id)}' kicked");
                                byte[] data = DisconnectionPocket.ConstructSingle("Server", "Kicked");
                                _clientManager.Send(id, data);
                                _clientManager.DeleteClient(id);
                            }
                            break;
                        }
                }
            }
        }

        static void PocketListener_OnAccept(int id)
        {
            //Console.WriteLine("[SERVER] <--- [Accepted] from [Client]: {0}", _clientManager.GetClientName(id));
            _clientManager.UpdateAcceptState(id, true);
        }

        static void PocketListener_OnConnect(ConnectionPocket pocket, Socket client, int id)
        {
            if (id > -1 && id < _clientManager.GetAvailibleID())
            {
                if (_clientManager.GetClientState(id) == (int)ClientStateEnum.Disconnected)
                    _clientManager.ReplaceClient(client, id);
                else
                    _clientManager.UpdateClientSocket(id, client);
                Console.WriteLine($"[SERVER]: '{pocket.Name}' reconnected"); // {pocket.Message}
            }
            else
            {
                _clientManager.AddClient(client, pocket.Name);
                Console.WriteLine($"[SERVER]: '{pocket.Name}' connected"); // pocket.Message
            }
            byte[] data = ConnectionPocket.ConstructSingle("Server", "Successfull");
            _clientManager.Send(id, data, false);
        }

        static void PocketListener_OnDisconnect(DisconnectionPocket pocket, int id)
        {
            Console.WriteLine($"[SERVER]: '{pocket.Name}' disconnected ({pocket.Message})");
            if (_clientManager.GetSocket(id) != null)
            {
                byte[] data = DisconnectionPocket.ConstructSingle("Server", "Successfull");
                _clientManager.Send(id, data);
            }
            //do
            Thread.Sleep(50);
            //while (!_clientManager.GetClientCallback(id));
            _clientManager.DeleteClient(id);
        }

        static void ClientManager_OnLostConnection(int id)
        {
            Console.WriteLine($"[SERVER]: '{_clientManager.GetClientName(id)}' disconnected (Timed out)");
            _clientManager.DeleteClient(id);
        }

        static void PocketListener_OnChatMessage(ChatMessagePocket pocket, int id)
        {
            Console.WriteLine($"[SERVER] <--- [Client]: {pocket.Name} [Message]: {pocket.Message}");

            /// Resend example (like chat message)

            byte[] data = ChatMessagePocket.ConstructSingle(pocket.Name, pocket.Message);
            _clientManager.SendToAllExcept(data, id);
        }
    }
}