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
        public ConcurrentDictionary<long, MyClient> clients = new ConcurrentDictionary<long, MyClient>();

        public int GetNextID()
        {
            return _id;
        }

        public Socket GetSocket(long key)
        {
            clients.TryGetValue(key, out MyClient tmp);
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
            clients.TryAdd(_id, newClient);
            _id++;
        }

        public void DeleteClient(Socket client)
        {
            clients.TryRemove(FindClient(client), out _);
            _id--;
        }

        public void DeleteClient(int id)
        {
            clients.TryRemove(id, out _);
            _id--;
        }

        public long FindClient(Socket client)
        {
            foreach (var existClient in clients)
            {
                if (existClient.Value.client == client)
                    return existClient.Key;
            }
            return -1;
        }

        public string GetClientName(Socket client)
        {
            clients.TryGetValue(FindClient(client), out MyClient tmp);
            return tmp.name;
        }

        public string GetClientName(int id)
        {
            clients.TryGetValue(id, out MyClient tmp);
            return tmp.name;
        }
    }
}
