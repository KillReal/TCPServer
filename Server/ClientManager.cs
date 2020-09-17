using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    public class ClientManager
    {
        private int id;

        private struct MyClient
        {
            public int id;
            public string name;
            public Socket client;
            public byte[] buffer;
        };
        private ConcurrentDictionary<long, MyClient> clients = new ConcurrentDictionary<long, MyClient>();

        public void AddClient(Socket client, string name)
        {
            MyClient newClient = new MyClient
            {
                id = id,
                name = name,
                client = client
            };
            clients.TryAdd(id, newClient);
            id++;
        }

        public void DeleteClient(Socket client)
        {
            clients.TryRemove(FindClient(client), out _);
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
    }
}
