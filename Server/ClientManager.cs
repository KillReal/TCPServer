using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    public class ClientManager
    {
        private int _id;

        public struct MyClient
        {
            public int id;
            public string name;
            public Socket client;
        };
        public ConcurrentDictionary<long, MyClient> _clients = new ConcurrentDictionary<long, MyClient>();

        public int GetNextID()
        {
            return _id;
        }

        public Socket GetSocket(long key)
        {
            _clients.TryGetValue(key, out MyClient tmp);
            return tmp.client;
        }
        public void AddClient(Socket client, string name)
        {
            MyClient newClient = new MyClient
            {
                id = _id,
                name = name,
                client = client
            };
            _clients.TryAdd(_id, newClient);
            _id++;
        }

        public void DeleteClient(Socket client)
        {
            DeleteClient((int)FindClient(client));
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
                if (existClient.Value.client == client)
                    return existClient.Key;
            }
            return -1;
        }

        public string GetClientName(Socket client)
        {
            return GetClientName((int)FindClient(client));
        }

        public string GetClientName(int id)
        {
            _clients.TryGetValue(id, out MyClient tmp);
            return tmp.name;
        }

        public void SendPocketToAll(byte[] pocket)
        {
            for (int i = 0; i < _id; i++)
                    _clients[i].client.Send(pocket);
        }

        public void SendPocketToAllExcept(byte[] pocket, Socket excepted_client)
        {
            SendPocketToAllExcept(pocket, (int)FindClient(excepted_client));
        }

        public void SendPocketToAllExcept(byte[] pocket, int excepted_id)
        {
            for (int i = 0; i < _id; i++)
                if (i != excepted_id)
                    _clients[i].client.Send(pocket);
        }
    }
}
