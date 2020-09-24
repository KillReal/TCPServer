using Server.Enums;
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
            public Socket socket;
            public byte[] send_buffer;
            public byte[] recieve_buffer;
            public int state;
        };
        public ConcurrentDictionary<long, MyClient> _clients = new ConcurrentDictionary<long, MyClient>();

        public int GetAvailibleID()
        {
            return _id;
        }

        private MyClient GetClient(int id)
        {
            _clients.TryGetValue(id, out MyClient tmp);
            return tmp;
        }

        public void SetAcceptState(int id, bool accept)
        {
            MyClient tmp = GetClient(id);
            if (accept)
                tmp.state = (int)ClientStateEnum.AskedForAccept;
            else
                tmp.state = (int)ClientStateEnum.Idle;
        }

        public void Send(int id, byte[] data)
        {
            MyClient tmp = GetClient(id);
            tmp.send_buffer = data;
            if (tmp.socket != null)
                tmp.socket.Send(data);
        }

        public void Recieve(int id, ref byte[] data)
        {
            MyClient tmp = GetClient(id);
            tmp.socket.Receive(data);
            tmp.recieve_buffer = data;
        }

        public Socket GetSocket(long key)
        {
            _clients.TryGetValue(key, out MyClient tmp);
            return tmp.socket;
        }

        public void AddClient(Socket socket, string name)
        {
            MyClient newClient = new MyClient
            {
                id = _id,
                name = name,
                socket = socket
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
            _clients.TryGetValue(id, out MyClient tmp);
            return tmp.name;
        }
    }
}
