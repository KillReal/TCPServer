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
        static ClientManager clientManager = new ClientManager();
        static GameManager gameManager = new GameManager();
        static Settings settings = new Settings();
        static void Main(string[] args)
        {
            PocketHandler.OnMessageAccepted += Client_OnAccept;
            PocketHandler.OnConnection += Client_OnConnect;
            PocketHandler.OnChatMessage += Client_OnChatMessage;
            PocketHandler.onClientDisconnect += Client_OnDisconnect;
            ClientManager.onClientLostConnection += Client_OnLostConnection;

            clientManager.Init(settings);
            gameManager.Init(clientManager);
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
                            Console.WriteLine($"[SERVER] ---> [All Clients]: [Message]: ping");
                            clientManager.SendToAll(new ChatMessagePocket("Server", "Test"));
                            break;
                        }

                    case "list":
                        {
                            Console.WriteLine("   List of all connected clients");
                            for (int i = 0; i < clientManager.ID_list.Count; i++)
                                Console.WriteLine("   " + clientManager.GetClientInfo(clientManager.ID_list[i]));
                            break;
                        }

                    case "clr":
                        Console.Clear();
                        break;
                        
                    case "kick":
                        {
                            Console.Write("Enter client id: ");
                            int id = Convert.ToInt32(Console.ReadLine());
                            if (id > -1 && id < clientManager.GetMaxID())
                            {
                                Console.WriteLine($"[SERVER]: Client '{clientManager.GetClientName(id)}' kicked");
                                clientManager.Send(id, new DisconnectionPocket("Server", "Kicked"));
                                clientManager.DeleteClient(id);
                            }
                            break;
                        }
                }
            }
        }

        static void Client_OnAccept(int id)
        {
            //Console.WriteLine("[SERVER] <--- [Accepted] from [Client]: {0}", _clientManager.GetClientName(id));
            clientManager.UpdateAcceptState(id, true);
        }

        static void Client_OnConnect(ConnectionPocket pocket, Socket client, int id)
        {
            if (settings.MaxClients <= clientManager.GetAvailibleID())
                return;
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

            if (clientManager.GetAvailibleID() == settings.MaxClients)
                gameManager.StartGame(clientManager.ID_list);
        }

        static void Client_OnDisconnect(DisconnectionPocket pocket, int id)
        {
            Console.WriteLine($"[SERVER]: '{pocket.Name}' disconnected ({pocket.Message})");
            if (clientManager.GetSocket(id) != null)
                clientManager.Send(id, new DisconnectionPocket("Server", "Successfull"));
            //do
            Thread.Sleep(50);
            //while (!_clientManager.GetClientCallback(id));
            clientManager.DeleteClient(id);
        }

        static void Client_OnLostConnection(int id)
        {
            Console.WriteLine($"[SERVER]: '{clientManager.GetClientName(id)}' disconnected (Timed out)");
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