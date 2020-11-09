using Server.Enums;
using Server.Pockets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public class ClientManager
    {
        private static Settings _settings;
        public static Action<int> onClientLostConnection;

        public List<int> ID_list;

        private int _id;

        public void SetSettings(Settings settings)
        {
            PocketHandler.onPingRecieved += PocketListener_OnPing;
            ID_list = new List<int>();
            _settings = settings;
        }
        public struct MyClient
        {
            public int id;
            public string name;
            public Socket socket;
            public bool callback;
            public int buffer_id;
            public byte[] send_buffer;
            public byte[] recieve_buffer;
            public int state;
            public int ping;
            public Timer timer;
        };
        public ConcurrentDictionary<long, MyClient> _clients = new ConcurrentDictionary<long, MyClient>();

        public int GetMaxID()
        {
            int max = 0;
            for (int i = 1; i < ID_list.Count; i++)
                if (ID_list[i] > max)
                    max = ID_list[i];
            return max;
        }

        public int GetAvailibleID()
        {
            /*if (ID_list.Count > 0)
            {
                int min = ID_list[0];
                for (int i = 1; i < ID_list.Count(); i++)
                {
                    if (ID_list[i] < min)
                    {
                        min = ID_list[i];
                    }
                }
                if (min > 0)
                {
                    min--;
                    return min;
                }    
            }*/
            return _id;
        }

        public int GetLastBufferID(int id)
        {
            _clients.TryGetValue(id, out MyClient client);
            return client.buffer_id;
        }

        public string GetClientInfo(int id)
        {
            MyClient client = GetClient(id);
            return "[" + id.ToString() + "]   Name: " + client.name + "   State: " + Enum.GetName(typeof(ClientStateEnum), client.state) + 
                "   Callback: " + client.callback + "   Ping: " + client.ping + " ms";
        }

        private MyClient GetClient(int id)
        {
            _clients.TryGetValue(id, out MyClient client);
            return client;
        }

        private void WaitForClientDelete(int id)
        {
            Console.WriteLine("[SERVER]: '{0}' doesn't responding, wait {1} sec reconnect timeout....", GetClientName(id), _settings.ReconnectionTimeOut);
            for (int i = 0; i < _settings.ReconnectionTimeOut * 10; i++)
            {
                if (GetClient(id).socket == null)
                    return;
                if (GetClient(id).callback)
                {
                    ToggleConnectionState(id, true);
                    return;
                }
                Thread.Sleep(100);
            }
            onClientLostConnection?.Invoke(id);
        }

        private void WaitForClientAccept(object obj)
        {
            int id = (int)obj;
            WaitForClientDelete(id);
        }

        public void ToggleConnectionState(int id, bool forceConnected = false)
        {
            MyClient client = GetClient(id);
            if (client.socket == null)
                return;
            if (client.state == (int)ClientStateEnum.Connected && !forceConnected)
            {
                client.state = (int)ClientStateEnum.Disconnected;
                _clients[id] = client;
                UpdateAcceptState(id, false);
            }
            else
            {
                client.state = (int)ClientStateEnum.Connected;
                _clients[id] = client;
                UpdateAcceptState(id, true);
            }
        }

        public void UpdateAcceptState(int id, bool accept)
        {
            MyClient client = GetClient(id);
            if (client.socket == null)
                return;
            if (!accept)
            {
                client.callback = false;
                if (client.timer == null)
                    client.timer = new Timer(new TimerCallback(WaitForClientAccept), id, _settings.ConnectionTimeOut * 1000, -1);
                _clients[id] = client;
            }
            else           
            {
                client.callback = true;
                if (client.timer != null)
                {
                    client.timer.Dispose();
                    client.timer = null;
                }
                _clients[id] = client;
            }
        }

        private void SendTask(int id, byte[] data, int data_id, bool wait_accept)
        {
            while (GetClient(id).socket != null)
            {
                MyClient client = GetClient(id);
                if (client.callback)
                {
                    client.send_buffer = data;
                    client.buffer_id = data_id;
                    _clients[id] = client;
                    if (wait_accept)
                    {
                        //Console.WriteLine("[SERVER] ---> [CLIENT]: sended data to '{0}'", GetClientName(id));
                        UpdateAcceptState(id, false);
                    }
                    if (_settings.EncryptionEnabled)
                        data = Encryption.Encrypt(data);
                    if (data == null)
                    {
                        if (_settings.ExceptionPrint)
                            Console.WriteLine("[ERROR]: Decryption failed");
                    }
                    else
                    {
                        try
                        {
                            client.socket.Send(data);
                        }
                        catch (Exception exception)
                        {
                            if (_settings.ExceptionPrint)
                                Console.WriteLine("[ERROR]: " + exception.Message + " " + exception.InnerException);
                        }
                    }
                    return;
                }
                Thread.Sleep(100);
            }
        }

        public void Send(int id, byte[] data, bool wait_accept = false)
        {
            int data_id = (int)DateTime.Now.Ticks;
            byte[] header = new MainHeader(_settings.PocketHash, data_id).ToBytes();
            data = Utils.ConcatBytes(header, data);
            (new Task(() => SendTask(id, data, data_id, wait_accept))).Start();
        }

        public void Send(int id, BasePocket pocket, bool wait_accept = false)
        {
            int data_id = (int)DateTime.Now.Ticks;
            var header = new MainHeader(_settings.PocketHash, data_id);
            byte[] data = Utils.ConcatBytes(header, pocket);
            (new Task(() => SendTask(id, data, data_id, wait_accept))).Start();
        }

        public void SetRecieve(int id, byte[] data)
        {
            MyClient client = GetClient(id);
            client.recieve_buffer = data;
            UpdateAcceptState(id, true);
            _clients[id] = client;
        }

        public Socket GetSocket(int id)
        {
            _clients.TryGetValue(id, out MyClient client);
            return client.socket;  
        }

        private void PocketListener_OnPing(PingPocket pocket, int id)
        {
            MyClient client = GetClient(id);
            client.ping = ((int)DateTime.Now.Ticks - pocket.Tick) / 10000;
            _clients[id] = client;
        }

        public void ClientPinger(int id)
        {
            do
            {
                Thread.Sleep(_settings.PingTimerFreq * 1000);
                MyClient client = GetClient(id);
                if (client.callback)
                    Send(id, PingPocket.ConstructSingle((int)DateTime.Now.Ticks, client.ping));
            } while (GetClient(id).socket != null);
        }

        public void AddClient(Socket socket, string name)
        {
            MyClient newClient = new MyClient
            {
                id = _id,
                name = name,
                socket = socket,
                callback = true,
                state = (int)ClientStateEnum.Connected
            };
            _clients.TryAdd(_id, newClient);
            ID_list.Add(_id);
            (new Task(() => ClientPinger(ID_list.Last()))).Start();
            _id++;
        }

        public void ReplaceClient(Socket socket, int id)
        {
            _clients.TryGetValue(id, out MyClient client);
            client.socket = socket;
            _clients[id] = client;
            ToggleConnectionState(id, true);
        }

        public void DeleteClient(int id)
        {
            MyClient client = GetClient(id);
            if (client.timer != null)
            {
                client.timer.Dispose();
                client.timer = null;
            }
            _clients.TryRemove(id, out _);
            ID_list.Remove(id);
            if (ID_list.Count == 0)
                _id = 0;
        }

        public int FindClient(Socket client)
        {
            foreach (var existClient in _clients)
            {
                if (existClient.Value.socket == client)
                    return (int)existClient.Key;
            }
            return -1;
        }

        public string GetClientName(int id)
        {
            _clients.TryGetValue(id, out MyClient client);
            if (client.name == null)
                return "unknown";
            return client.name;
        }

        public int GetClientState(int id)
        {
            _clients.TryGetValue(id, out MyClient client);
            return client.state;
        }

        public long FindClient(string name)
        {
            foreach (var existClient in _clients)
            {
                if (existClient.Value.name == name)
                    return existClient.Key;
            }
            return -1;
        }
    }
}
