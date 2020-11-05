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

        private int _id;

        public void SetSettings(Settings settings)
        {
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
        };
        public static ConcurrentDictionary<long, MyClient> _clients = new ConcurrentDictionary<long, MyClient>();

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
            return "[" + id.ToString() + "] name: " + client.name + " state: " + Enum.GetName(typeof(ClientStateEnum), client.state);
        }

        private MyClient GetClient(int id)
        {
            _clients.TryGetValue(id, out MyClient client);
            return client;
        }

        private void WaitForClientDelete(int id)
        {
            int counter = 0;
            while (counter < 1)
            {
                //MyClient client = GetClient(id);
                if (GetClient(id).callback)
                    return;
                counter++;
                Thread.Sleep(10000);
            }
            onClientLostConnection?.Invoke(id);
        }

        private void WaitForClientAccept(int id)
        { 
            int counter = 0;
            while (counter < 1)
            {
                //MyClient client = GetClient(id);
                if (GetClient(id).callback)
                    return;
                counter++;
                Thread.Sleep(10000);
            }
            Task waitTask = new Task(() => WaitForClientDelete(id));
            waitTask.Start();
        }

        public void SetConnectionState(int id)
        {
            MyClient client = GetClient(id);
            if (client.state == (int)ClientStateEnum.Connected)
                client.state = (int)ClientStateEnum.Disconnected;
            else
                client.state = (int)ClientStateEnum.Connected;
            _clients[id] = client;
        }

        public void SetAcceptState(int id, bool accept = false)
        {
            MyClient client = GetClient(id);
            if (!accept)
            {
                client.callback = false;
                client.state = (int)ClientStateEnum.Disconnected;
                _clients[id] = client;
                Task waitTask = new Task(() => WaitForClientAccept(id));
                waitTask.Start();
            }
            else
            {
                client.callback = true;
                client.state = (int)ClientStateEnum.Connected;
            }
            _clients[id] = client;
        }

        public void SendTask(MyClient client, byte[] data, int data_id)
        {
            
        }

        public void Send(int id, byte[] data)
        {
            int data_id = (int)DateTime.Now.Ticks;
            byte[] header = MainHeader.Construct(_settings.PocketHash, data_id);
            data = Utils.ConcatByteArrays(header, data);
            MyClient client = GetClient(id);
            if (client.socket != null)
            {
                client.send_buffer = data;
                client.buffer_id = data_id;
                client.socket.Send(data);
            }
        }

        public int Recieve(int id, ref byte[] data)
        {
            MyClient client = GetClient(id);
            if (client.socket != null)
            {
                int size = client.socket.Receive(data);
                client.recieve_buffer = data;
                return size;
            }
            return 0;
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
                state = 1
            };
            _clients.TryAdd(_id, newClient);
            _id++;
        }

        public void ReplaceClient(Socket socket, int id)
        {
            _clients.TryGetValue(id, out MyClient client);
            client.socket = socket;
            _clients[id] = client;
            SetAcceptState(id, true);
        }

        public void DeleteClient(int id)
        {
            _clients.TryRemove(id, out _);
            _id--;
        }

        public long FindClient(Socket client)
        {
            foreach (var existClient in _clients)
            {
                if (existClient.Value.socket == client)
                    return existClient.Key;
            }
            return -1;
        }

        public string GetClientName(int id)
        {
            _clients.TryGetValue(id, out MyClient client);
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
