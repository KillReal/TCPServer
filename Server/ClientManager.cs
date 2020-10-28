using Server.Enums;
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
        public static Action<int> onClientLostConnection;

        private int _id;

        public struct MyClient
        {
            public int id;
            public string name;
            public Socket socket;
            public bool callback;
            public byte[] send_buffer;
            public byte[] recieve_buffer;
            public int state;
        };
        public static ConcurrentDictionary<long, MyClient> _clients = new ConcurrentDictionary<long, MyClient>();

        public int GetAvailibleID()
        {
            return _id;
        }

        public string GetClientInfo(int id)
        {
            return "[" + id.ToString() + "] name: " + _clients[id].name + " state: " + Enum.GetName(typeof(ClientStateEnum), _clients[id].state);
        }

        private MyClient GetClient(int id)
        {
            _clients.TryGetValue(id, out MyClient client);
            return client;
        }

        private void WaitForClientDelete(int id)
        {
            MyClient client = GetClient(id);
            int counter = 0;
            while (counter < 3)
            {
                if (client.callback)
                    return;
                counter++;
                Thread.Sleep(1000);
            }
            onClientLostConnection?.Invoke(id);
        }

        private void WaitForClientAccept(int id)
        {
            MyClient client = GetClient(id);
            int counter = 0;
            while (counter < 3)
            {
                if (client.callback)
                    return;
                counter++;
                Thread.Sleep(1000);
            }
            SetConnectionState(id);
            Task waitTask = new Task(() => WaitForClientDelete(id));
            waitTask.Start();
        }

        public void SetConnectionState(int id)
        {
            MyClient client = GetClient(id);
            if (client.state != (int)ClientStateEnum.Disconnected)
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
                SetConnectionState(id);
                Task waitTask = new Task(() => WaitForClientAccept(id));
                waitTask.Start();
            }
            else
                client.callback = true;
        }

        public void Send(int id, byte[] data)
        {
            MyClient client = GetClient(id);
            client.send_buffer = data;
            if (client.socket != null)
                client.socket.Send(data);
        }

        public int Recieve(int id, ref byte[] data)
        {
            MyClient client = GetClient(id);
            int size = client.socket.Receive(data);
            client.recieve_buffer = data;
            return size;
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
    }
}
