using Server.Enums;
using Server.Pockets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        public List<TimerCallback> TIMER_list;

        private int _id;

        public void SetSettings(Settings settings)
        {
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
            public Timer timer;
        };
        public ConcurrentDictionary<long, MyClient> _clients = new ConcurrentDictionary<long, MyClient>();

        public int GetAvailibleID()
        {
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
            return "[" + id.ToString() + "] name: " + client.name + " state: " + Enum.GetName(typeof(ClientStateEnum), client.state) + " callback: " + client.callback;
        }

        private MyClient GetClient(int id)
        {
            _clients.TryGetValue(id, out MyClient client);
            return client;
        }

        private void WaitForClientDelete(int id)
        {
            for (int i = 0; i < _settings.ConnectionTimeOut * 10; i++)
            {
                if (GetClient(id).socket == null)
                    return;
                if (GetClient(id).callback)
                {
                    ToggleConnectionState(id);
                    return;
                }
                Thread.Sleep(100);
            }
            Console.WriteLine("[SERVER]: '{0}' doesn't responding...", GetClientName(id));
            onClientLostConnection?.Invoke(id);
        }

        private void WaitForClientAccept(object obj)
        {
            int id = (int)obj;
            WaitForClientDelete(id);
        }

        public void ToggleConnectionState(int id)
        {
            MyClient client = GetClient(id);
            if (client.socket == null)
                return;
            if (client.state == (int)ClientStateEnum.Connected)
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
                client.timer = new Timer(new TimerCallback(WaitForClientAccept), id, 500, _settings.ConnectionTimeOut * 1000);
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
                        Console.WriteLine("[SERVER] ---> [CLIENT]: sended data to '{0}'", GetClientName(id));
                        UpdateAcceptState(id, false);
                    }
                    client.socket.Send(data);
                    return;
                }
                Thread.Sleep(100);
            }
        }

        public void Send(int id, byte[] data, bool wait_accept = false)
        {
            int data_id = (int)DateTime.Now.Ticks;
            byte[] header = MainHeader.Construct(_settings.PocketHash, data_id);
            data = Utils.ConcatByteArrays(header, data);
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
            _id++;
        }

        public void ReplaceClient(Socket socket, int id)
        {
            _clients.TryGetValue(id, out MyClient client);
            client.socket = socket;
            _clients[id] = client;
            ToggleConnectionState(id);
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
