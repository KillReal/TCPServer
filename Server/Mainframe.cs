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
        static ClientManager clientManager = new ClientManager();
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern short GetAsyncKeyState(int vKey);
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        static private bool IsKeyPressed(int result)
        {
            if (GetConsoleWindow() != GetForegroundWindow())
                return false;
            if (result < 0 && (result & 0x01) == 0x01)
                return true;
            return false;
        }
        static void Main(string[] args)
        {
            PocketHandler.OnConnectionPocket += PocketListener_OnConnect;
            PocketHandler.OnStringPocket += PocketListener_OnString;
            PocketHandler.onClientDisconnect += PocketListener_OnDisconnect;

            Settings settings = new Settings();
            PocketListener pocketListener = new PocketListener(clientManager, settings);
            Console.WriteLine("[INFO]:  Server is starting...");
            try
            {
                pocketListener.Start();
            }
            catch (Exception ex)
            {
                //Console.WriteLine("[ERROR]: " + ex.ToString());
            }
            finally
            {
                Console.WriteLine("[INFO]:  Server started successfully! (Press <Q> for stop server)");
                while (true)
                {
                    //exit
                    if (IsKeyPressed(GetAsyncKeyState('Q')))
                    {
                        Console.WriteLine("[INFO]:  Server is stopping...");
                        pocketListener.Stop();
                        Thread.Sleep(100);
                        Console.WriteLine("[INFO]:  Server is successfully stopped! Press any key to exit.");
                        Console.ReadKey();
                        Environment.Exit(0);
                    }
                    // TEST messages to all clients
                    if (IsKeyPressed(GetAsyncKeyState('T')))
                    {
                        HeaderPocket header = new HeaderPocket
                        {
                            Count = 1,
                            Type = (int)PocketEnum.String
                        };
                        StringPocket str = new StringPocket
                        {
                            StringField = "test"
                        };
                        byte[] data = Utils.ConcatByteArrays(header.ToBytes(), str.ToBytes());
                        Console.WriteLine("[SERVER] ---> [All Clients]: [Message]: {0}", str.StringField);
                        clientManager.SendPocketToAll(data);
                    }
                    Thread.Sleep(100);
                }
            }
        }

        private static void PocketListener_OnConnect(ConnectionPocket pocket, Socket client)
        {
            Console.WriteLine("[SERVER] <--- [Client]: {0} connected", pocket.Name, pocket.Message);
            clientManager.AddClient(client, pocket.Name);
            PocketSender.SendAcceptedToClient(client);
        }
        private static void PocketListener_OnDisconnect(int id)
        {
            Console.WriteLine("[SERVER] <--- [Client]: {0} disconnected", clientManager.GetClientName(id));
            clientManager.DeleteClient(id);
            PocketSender.SendAcceptedToClient(clientManager.GetSocket(id));
        }
        private static void PocketListener_OnString(StringPocket pocket, int id)
        {
            Console.WriteLine("[SERVER] <--- [Client]: {0} [Message]: {1}", clientManager.GetClientName(id),  pocket.StringField);
            PocketSender.SendAcceptedToClient(clientManager.GetSocket(id));

            /// Resend example (like chat message)

            byte[] data = ChatMessagePocket.Construct(clientManager.GetClientName(id), pocket.StringField);
            clientManager.SendPocketToAllExcept(data, id);
        }
    }
}