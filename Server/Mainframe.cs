using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Server.Pockets;

namespace Server
{
    class Program
    {
        static ClientManager clientManager = new ClientManager();
        [System.Runtime.InteropServices.DllImport("User32.dll")]
        public static extern short GetAsyncKeyState(int vKey);
        static void Main(string[] args)
        {
            PocketHandler.OnConnectionPocket += PocketListener_OnConnection;
            PocketHandler.OnStringPocket += PocketListener_OnString;

            Settings settings = new Settings();
            PocketListener pocketListener = new PocketListener(clientManager, settings);
            Console.WriteLine("[INFO]:  Server is starting...");
            try
            {
                pocketListener.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                Console.WriteLine("[INFO]:  Server started successfully! (Press <Q> for stop server)");
                short result = 0;
                while (result == 0)
                {
                    result = GetAsyncKeyState('Q');
                    if (result < 0 && (result & 0x01) == 0x01)
                    {
                        Console.WriteLine("[INFO]:  Server is stopping...");
                        pocketListener.Stop();
                        Thread.Sleep(100);
                        Console.WriteLine("[INFO]:  Server is successfully stopped! Press any key to exit.");
                        Console.ReadKey();
                    }
                    Thread.Sleep(100);
                }
            }
        }

        private static void PocketListener_OnConnection(ConnectionPocket pocket, Socket client)
        {
            Console.WriteLine("[СONNECT]: Client:{0} connected with msg:{1}", pocket.Name, pocket.Message);
            clientManager.AddClient(client, pocket.Name);
            PocketSender.SendAcceptedToClient(client);
        }

        private static void PocketListener_OnString(StringPocket pocket, Socket client)
        {
            string name = clientManager.GetClientName(client);
            Console.WriteLine("[STRING]: Client:{0}, msg:{1}", name,  pocket.StringField);
            PocketSender.SendAcceptedToClient(client);
        }
    }
}