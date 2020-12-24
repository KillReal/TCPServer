using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Server.Pockets;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

namespace Server
{
    class Program
    {
        static ClientManager clientManager = new ClientManager();
        static Settings settings = new Settings();
        static void Main(string[] args)
        {
            PocketHandler.OnMessageAccepted += Client_OnAccept;
            PocketHandler.OnConnection += Client_OnConnect;
            PocketHandler.OnChatMessage += Client_OnChatMessage;
            PocketHandler.onClientDisconnect += Client_OnDisconnect;
            ClientManager.onClientLostConnection += Client_OnLostConnection;
            PocketHandler.onPlayState += Client_OnPlayState;

            settings.Init();
            clientManager.Init(settings);
            PocketListener pocketListener = new PocketListener(clientManager, settings);
            Encryption.Init(settings);
            PocketHandler.Init(clientManager, settings);
            Console.WriteLine("[INFO]:  Server is starting...");
            try
            {
                pocketListener.Start();
            }
            catch (Exception exception)
            {
                if (settings.ExceptionPrint)
                    Console.WriteLine($"[ERROR]:  {exception.Message } - {exception.InnerException}");
            }
            Console.WriteLine("[INFO]:  Server started successfully! (Type <quit> to console for stop server, or <help> to see command list)");

            while (true)
            {
                int id = -1;
                switch (Console.ReadLine())
                {
                    case "quit":
                        Console.WriteLine("[INFO]:  Server is stopping...");
                        pocketListener.Stop();
                        settings.SaveChanges();
                        Console.WriteLine("[INFO]:  Server is successfully stopped! Press any key to exit.");
                        Console.ReadKey();
                        return;

                    case "ping":
                        {
                            Console.WriteLine($"[SERVER] ---> [All Clients]: [Message]: ping");
                            clientManager.SendToAll(new ChatMessagePocket("Admin", "Test"));
                            break;
                        }

                    case "list":
                        {
                            Console.WriteLine("   List of all connected clients");
                            for (int i = 0; i < clientManager.ID_list.Count; i++)
                                Console.WriteLine("   " + clientManager.GetClientInfo(clientManager.ID_list[i]));
                            break;
                        }

                    case "banlist":
                        {
                            Console.WriteLine("   List of all banned clients");
                            for (int i = 0; i < settings.bannedClients.Count; i++)
                                Console.WriteLine($"   Name: {settings.bannedClients[i].name}    Ip: {settings.bannedClients[i].ip}");
                            break;
                        }

                    case "history":
                        {
                            Console.WriteLine("   List of disconnected clients");
                            for (int i = 0; i < clientManager._history.Count; i++)
                                Console.WriteLine("   " + clientManager.GetHistoryClientInfo(i));
                            break;
                        }

                    case "clr":
                        Console.Clear();
                        break;
                        
                    case "kick":
                        {
                            Console.Write("Enter client id: ");
                            id = Convert.ToInt32(Console.ReadLine());
                            if (id > -1 && id < clientManager.GetMaxID())
                            {
                                Console.WriteLine($"[SERVER]: Client '{clientManager.GetClientName(id)}' kicked");
                                clientManager.Send(id, new DisconnectionPocket("Admin", "You had kicked"));
                                clientManager.DeleteClientFromSession(id);
                                clientManager.DeleteClient(id);
                            }
                            break;
                        }
                    case "hban":
                        Console.Write("Enter client id: ");
                        id = Convert.ToInt32(Console.ReadLine());
                        if (id > -1 && id < clientManager._history.Count)
                        {
                            settings.AddBannedClient(clientManager._history[id].name, clientManager._history[id].ip);
                            if (clientManager.FindClient(clientManager._history[id].name) != -1)
                            {
                                clientManager.Send(id, new DisconnectionPocket("Admin", "You had banned"));
                                clientManager.DeleteClientFromSession(id);
                                clientManager.DeleteClient(id);
                            }
                        }
                        break;
                    case "ban":
                        {
                            Console.Write("Enter client id: ");
                            id = Convert.ToInt32(Console.ReadLine());
                            if (id > -1 && id < clientManager.GetMaxID())
                            {
                                settings.AddBannedClient(clientManager.GetClientName(id), clientManager.GetClientIP(id));
                                clientManager.Send(id, new DisconnectionPocket("Admin", "You had banned"));
                                clientManager.DeleteClientFromSession(id);
                                clientManager.DeleteClient(id);
                            }
                            break;
                        }
                    case "unban":
                        {
                            Console.Write("Enter client id: ");
                            id = Convert.ToInt32(Console.ReadLine());
                            if (id > -1 && id < settings.bannedClients.Count)
                                settings.DeleteBannedClient(id);
                            break;
                        }

                    default:
                        Console.WriteLine("Wrong command");
                        break;
                }
            }
        }

        private static void Client_OnPlayState(PlayStatePocket pocket, int id)
        {
            if (pocket.State == (int)ClientStateEnum.Ready)
            {
                if (pocket.SessionID > 0)
                {
                    // For future
                    // NotImplemented
                    // Asking prefered session
                }
                else
                {
                    int session_id = clientManager.AddClientToSession(id);
                    if (session_id > -1)
                        clientManager.Sessions[session_id].game.StartGame(clientManager.Sessions[session_id].players);
                }
            }
            else if (pocket.State == (int)ClientStateEnum.Exiting)
                clientManager.DeleteClientFromSession(id);
        }

        static void Client_OnAccept(int id)
        {
            //Console.WriteLine("[SERVER] <--- [Accepted] from [Client]: {0}", _clientManager.GetClientName(id));
            clientManager.UpdateAcceptState(id, true);
        }

        static void Client_OnConnect(ConnectionPocket pocket, Socket client, int id)
        {
            if (settings.MaxClients <= id)
            {
                client.Send((new DisconnectionPocket("Server", "Server slots maxed out")).ToBytes());
                Console.WriteLine($"[SERVER]: '{pocket.Name}' refuse to connect due to server slots maxed out");
                return;
            }
            if (settings.CheckClientBan(pocket.Name, ((IPEndPoint)client.RemoteEndPoint).Address.ToString()))
            {
                client.Send((new DisconnectionPocket("Server", "You banned on this server")).ToBytes());
                Console.WriteLine($"[SERVER]: '{pocket.Name}' refuse to connect due to ban");
                client.Disconnect(false);
                return;
            }
            if (id > -1 && id < clientManager.GetAvailibleID())
            {
                if (clientManager.GetClientState(id) == ClientStateEnum.Disconnected)
                    clientManager.ReplaceClient(client, id);
                else
                    clientManager.UpdateClientSocket(id, client);
                Console.WriteLine($"[SERVER]: '{pocket.Name}' reconnected"); // {pocket.Message}
            }
            else
            {
                clientManager.AddClient(client, pocket.Name);
                Console.WriteLine($"[SERVER]: '{pocket.Name}' connected"); // pocket.Message
            }
            clientManager.Send(id, new ConnectionPocket("Server", "Successfull"), false);
        }

        static void Client_OnDisconnect(DisconnectionPocket pocket, int id)
        {
            Console.WriteLine($"[SERVER]: '{pocket.Name}' disconnected ({pocket.Message})");
            if (clientManager.GetSocket(id) != null)
                clientManager.Send(id, new DisconnectionPocket("Server", "Successfull"));
            //do
            Thread.Sleep(50);
            //while (!_clientManager.GetClientCallback(id));
            clientManager.DeleteClientFromSession(id);
            clientManager.DeleteClient(id);
        }

        static void Client_OnLostConnection(int id)
        {
            Console.WriteLine($"[SERVER]: '{clientManager.GetClientName(id)}' disconnected (Timed out)");
            clientManager.DeleteClientFromSession(id);
            clientManager.DeleteClient(id);
        }

        static void Client_OnChatMessage(ChatMessagePocket pocket, int id)
        {
            Console.WriteLine($"[SERVER] <--- [Client]: {pocket.Name} [Message]: {pocket.Message}");

            /// Resend example (like chat message)

            clientManager.SendToAllExcept(new ChatMessagePocket(pocket.Name, pocket.Message), id);
        }
    }
}