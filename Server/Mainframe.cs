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
        static Options options = new Options();
        static GameManager gameManager = new GameManager();
        static void Main(string[] args)
        {
            PocketHandler.OnMessageAccepted += Client_OnAccept;
            PocketHandler.OnConnection += Client_OnConnect;
            PocketHandler.OnChatMessage += Client_OnChatMessage;
            PocketHandler.onClientDisconnect += Client_OnDisconnect;
            ClientManager.onClientLostConnection += Client_OnLostConnection;
            PocketHandler.onPlayState += Client_OnPlayState;
            options = DataManager.Init(options);
            clientManager.Init(options);
            gameManager.Init(clientManager);
            PocketListener pocketListener = new PocketListener(options);
            Encryption.Init(options);
            PocketHandler.Init(clientManager, options);
            DataManager.LogLine("[INFO]:  Server is starting...");
            try
            {
                pocketListener.Start();
            }
            catch (Exception exception)
            {
                DataManager.LogLine($"[ERROR]:  {exception.Message } - {exception.InnerException}", 2);
            }
            DataManager.LogLine("[INFO]:  Server started successfully! (Type <quit> to console for stop server, or <help> to see command list)");

            while (true)
            {
                int id = -1;
                string name = "";
                string ip = "";
                switch (Console.ReadLine())
                {
                    case "quit":
                        DataManager.LogLine("[INFO]:  Server is stopping...");
                        pocketListener.Stop();
                        for (int i = 0; i < clientManager.ID_list.Count; i++)
                            clientManager.Send(clientManager.ID_list[i], new DisconnectionPocket("Server", "Admin is stopping the server"));
                        Thread.Sleep(500);
                        while (clientManager.ID_list.Count > 0)
                        {
                            DataManager.LogLine($"[SERVER]: '{clientManager.GetClientName(clientManager.ID_list[0])}' disconnected (Server is shutting down)");
                            DataManager.LogLine($"[SERVER]: Client info: {clientManager.GetClientInfo(clientManager.ID_list[0])}", 1);
                            clientManager.DeleteClientFromSession(clientManager.ID_list[0]);
                            clientManager.DeleteClient(clientManager.ID_list[0]);
                        }
                        DataManager.SaveChanges();
                        DataManager.LogLine("[INFO]:  Server is successfully stopped! Press any key to exit.");
                        Console.ReadKey();
                        return;

                    case "ping":
                        {
                            DataManager.LogLine($"[SERVER] ---> [All Clients]: [Message]: ping");
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
                            for (int i = 0; i < DataManager.bannedClients.Count; i++)
                                Console.WriteLine($"    [{i}]   Name: {DataManager.bannedClients[i].name}    Ip: {DataManager.bannedClients[i].ip}");
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
                                DataManager.LogLine($"[SERVER]: Client '{clientManager.GetClientName(id)}' kicked");
                                DataManager.LogLine($"[CLIENT-INFO]: {clientManager.GetClientInfo(id)}", 1);
                                clientManager.Send(id, new DisconnectionPocket("Admin", "You had kicked"));
                                clientManager.DeleteClientFromSession(id);
                                clientManager.DeleteClient(id);
                            }
                            break;
                        }
                    case "mban":
                        Console.Write("Enter client name: ");
                        name = Console.ReadLine();
                        Console.Write("Enter client ip: ");
                        ip = Console.ReadLine();
                        DataManager.AddBannedClient(name, ip);
                        if (clientManager.FindClient(name) != -1)
                        {
                            clientManager.Send(id, new DisconnectionPocket("Admin", "You had banned"));
                            clientManager.DeleteClientFromSession(id);
                            clientManager.DeleteClient(id);
                        }
                        break;
                    case "ban":
                        {
                            Console.Write("Enter client id: ");
                            id = Convert.ToInt32(Console.ReadLine());
                            if (id > -1 && id < clientManager.GetMaxID())
                            {
                                DataManager.AddBannedClient(clientManager.GetClientName(id), clientManager.GetClientIP(id));
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
                            if (id > -1 && id < DataManager.bannedClients.Count)
                                DataManager.DeleteBannedClient(id);
                            break;
                        }
                    case "help":
                        {
                            Console.WriteLine("    [list] - print list of all connected clients\n    " +
                                "[banlist] - print list of all banned clients\n    " +
                                "[clr] - clear console\n    " +
                                "[kick] - kick client\n    " +
                                "[ban] - ban connected client by id from [list]\n    " +
                                "[mban] - manual client ban (for disconnected clients)\n    " +
                                "[unban] - unban client by id from [banlist]\n    " +
                                "[ping] - send test pocket to all clients");
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
            DataManager.LogLine($"[SERVER]: '{clientManager.GetClientName(id)}' change PlayState->{Enum.GetName(typeof(ClientStateEnum), pocket.State)}");
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
                        gameManager.StartGame(clientManager.Sessions[session_id].players);
                }
            }
            else if (pocket.State == (int)ClientStateEnum.Exiting)
                clientManager.DeleteClientFromSession(id);
        }

        static void Client_OnAccept(int id)
        {
            //DataManager.LogMessage("[SERVER] <--- [Accepted] from [Client]: {0}", _clientManager.GetClientName(id));
            clientManager.UpdateAcceptState(id, true);
        }

        static void Client_OnConnect(ConnectionPocket pocket, Socket client, int id)
        {
            if (options.MaxClients <= id)
            {
                client.Send((new DisconnectionPocket("Server", "Server slots maxed out")).ToBytes());
                DataManager.LogLine($"[SERVER]: '{pocket.Name}' refuse to connect due to server slots maxed out");
                DataManager.LogLine($"[CLIENT-INFO]: {clientManager.GetClientInfo(id)}", 1);
                return;
            }
            if (DataManager.CheckClientBan(pocket.Name, ((IPEndPoint)client.RemoteEndPoint).Address.ToString()))
            {
                client.Send((new DisconnectionPocket("Server", "You banned on this server")).ToBytes());
                DataManager.LogLine($"[SERVER]: '{pocket.Name}' refuse to connect due to ban");
                DataManager.LogLine($"[CLIENT-INFO]:  {clientManager.GetClientInfo(id)}", 1);
                client.Disconnect(false);
                return;
            }
            if (id > -1 && id < clientManager.GetAvailibleID())
            {
                if (clientManager.GetClientState(id) == ClientStateEnum.Disconnected)
                    clientManager.ReplaceClient(client, id);
                else
                    clientManager.UpdateClientSocket(id, client);
                DataManager.LogLine($"[SERVER]: '{pocket.Name}' reconnected"); // {pocket.Message}
                DataManager.LogLine($"[CLIENT-INFO]: {clientManager.GetClientInfo(id)}", 1);
            }
            else
            {
                clientManager.AddClient(client, pocket.Name);
                DataManager.LogLine($"[SERVER]: '{pocket.Name}' connected"); // pocket.Message
                DataManager.LogLine($"[CLIENT-INFO]: {clientManager.GetClientInfo(id)}", 1);
            }
            clientManager.Send(id, new ConnectionPocket("Server", "Successfull"), false);
        }

        static void Client_OnDisconnect(DisconnectionPocket pocket, int id)
        {
            DataManager.LogLine($"[SERVER]: '{pocket.Name}' disconnected ({pocket.Message})");
            DataManager.LogLine($"[SERVER]: Client info: {clientManager.GetClientInfo(id)}", 1);
            if (clientManager.GetSocket(id) != null)
                clientManager.Send(id, new DisconnectionPocket("Server", "Successfull"));
            //do
            Thread.Sleep(500);
            //while (!_clientManager.GetClientCallback(id));
            clientManager.DeleteClientFromSession(id);
            clientManager.DeleteClient(id);
        }

        static void Client_OnLostConnection(int id)
        {
            DataManager.LogLine($"[SERVER]: '{clientManager.GetClientName(id)}' disconnected (Timed out)");
            clientManager.DeleteClientFromSession(id);
            clientManager.DeleteClient(id);
        }

        static void Client_OnChatMessage(ChatMessagePocket pocket, int id)
        {
            DataManager.LogLine($"[SERVER] <--- [Client]: {pocket.Name} [Message]: {pocket.Message}");
            DataManager.LogLine($"[CLIENT-INFO]: {clientManager.GetClientInfo(id)}", 1);
            /// Resend example (like chat message)

            clientManager.SendToAllExcept(new ChatMessagePocket(pocket.Name, pocket.Message), id);
        }
    }
}