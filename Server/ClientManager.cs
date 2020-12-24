
using Server.Pockets;
using Server.GameLogic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;

namespace Server
{
    public class ClientManager
    {
        private static Settings settings;
        public static Action<int> onClientLostConnection;

        public List<int> ID_list;
        public List<Session> Sessions;
        public struct Session
        {
            public List<int> players;
            public GameManager game;
        };

        public void Init(Settings _settings)
        {
            PocketHandler.onPingRecieved += PocketListener_OnPing;
            ID_list = new List<int>();
            Sessions = new List<Session>();
            settings = _settings;
        }
        public struct MyClient
        {
            public int id;
            public int sid;
            public string ip;
            public string name;
            public Socket socket;
            public bool callback;
            public int pocket_id;
            public byte[] buffer;
            public byte[] send_buffer;
            public int last_state;
            public int state;
            public int ping;
            public Timer timer;
        };
        public ConcurrentDictionary<long, MyClient> _clients = new ConcurrentDictionary<long, MyClient>();
        public List<Settings.Client> _history = new List<Settings.Client>();

        public string GetClientIP(int id)
        {
            return GetClient(id).ip;
        }

        public bool GetClientCallback(int id)
        {
            _clients.TryGetValue(id, out MyClient client);
            return client.callback;
        }

        public int GetMaxID()
        {
            if (ID_list.Count > 0)
                return ID_list.Max() + 1;
            return 0;
        }

        public int GetAvailibleID()
        {
            int id = 0;
            while (ID_list.Contains(id))
                id++;
            if (id >= settings.MaxClients)
                return -1;
            return id;
        }

        public int GetLastPocketID(int id)
        {
            _clients.TryGetValue(id, out MyClient client);
            return client.pocket_id;
        }

        public string GetHistoryClientInfo(int id)
        {
            Settings.Client client = _history[id];
            return $"[{id}]   Name: {client.name}   Ip: {client.ip}";
        }

        public string GetClientInfo(int id)
        {
            MyClient client = GetClient(id);
            return $"[id:{id}][sid:{client.sid}]   Name: {client.name}   State: {Enum.GetName(typeof(ClientStateEnum), client.state)}   Callback:  {client.callback}   Ping: {client.ping} ms   Ip: {client.ip}";
        }

        private MyClient GetClient(int id)
        {
            _clients.TryGetValue(id, out MyClient client);
            return client;
        }

        private void WaitForClientDelete(int id)
        {
            Console.WriteLine("[SERVER]: '{0}' doesn't responding, wait {1} sec reconnect timeout....", GetClientName(id), settings.ReconnectionTimeOut);
            for (int i = 0; i < settings.ReconnectionTimeOut; i++)
            {
                MyClient client = GetClient(id);
                if (client.callback)
                {
                    ToggleConnectionState(id, true);
                    return;
                }
                else if (client.socket != null && client.send_buffer != null)
                {
                    client.socket.Send(client.send_buffer);
                }
                    
                Thread.Sleep(1000);
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
            if (client.state >= (int)ClientStateEnum.Connected && !forceConnected)
            {
                client.last_state = client.state;
                client.state = (int)ClientStateEnum.Disconnected;
                client.socket = null;
                _clients[id] = client;
                UpdateAcceptState(id, false);
            }
            else
            {
                if (client.last_state > 0)
                    client.state = client.last_state;
                else
                    client.state = (int)ClientStateEnum.Connected;
                _clients[id] = client;
                UpdateAcceptState(id, true);
            }
        }

        public void DeleteClientFromSession(int id)
        {
            for (int i = 0; i < Sessions.Count; i++)
            {
                if (Sessions[i].players.Contains(id))
                {
                    Sessions[i].players.Remove(id);
                    MyClient client = GetClient(id);
                    client.sid = -1;
                    client.state = (int)ClientStateEnum.Connected;
                    _clients[id] = client;
                    Console.WriteLine($"[SERVER]: Player '{GetClientName(id)}' left from session '{i}'");
                }
                if (Sessions[i].players.Count == 0)
                {
                    Sessions.RemoveAt(i);
                    Console.WriteLine($"[SERVER]: Session '{i}' is ends up");
                }
            }
        }

        public int AddClientToSession(int id)
        {
            bool found_open_session = false;
            for (int i = 0; i < Sessions.Count; i++)
            {
                if (Sessions[i].players.Count < settings.MaxSessionClients)
                {
                    Sessions[i].players.Add(id);
                    MyClient client = GetClient(id);
                    client.sid = i;
                    client.state = (int)ClientStateEnum.Playing;
                    _clients[id] = client;
                    Console.WriteLine($"[SERVER]: Player '{GetClientName(id)}' is joined session '{i}'");
                    found_open_session = true;
                    if (Sessions[i].players.Count == settings.MaxSessionClients)
                    {
                        Console.Write($"[SERVER]: Session '{i}' begin game (");
                        for (int j = 0; j < Sessions[i].players.Count; j++)
                            Console.Write($"'{GetClientName(Sessions[i].players[j])}'");
                        Console.WriteLine(")");
                        return i;                     
                    }
                }
            }
            if (!found_open_session)
            {
                Console.WriteLine($"[SERVER]: Session '{Sessions.Count}' is opened by '{GetClientName(id)}'");
                Session new_session = new Session
                {
                    players = new List<int> { id },
                    game = new GameManager()
                };
                new_session.game.Init(this);
                Sessions.Add(new_session);
                MyClient client = GetClient(id);
                client.sid = Sessions.Count - 1;
                client.state = (int)ClientStateEnum.Preparing;
                _clients[id] = client;
            }
            return -1;
        }

        public void UpdateAcceptState(int id, bool accept)
        {
            MyClient client = GetClient(id);
            if (!accept)
            {
                client.callback = false;
                if (client.timer == null)
                    client.timer = new Timer(new TimerCallback(WaitForClientAccept), id, settings.ConnectionTimeOut * 1000, -1);
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

        public byte[] GetBuffer(int id)
        {
            MyClient client = GetClient(id);
            return client.buffer;
        }

        public void AddBuffer(int id, byte[] data)
        {
            MyClient client = GetClient(id);
            client.buffer = Utils.ConcatBytes(client.buffer, data);
            _clients[id] = client;
        }

        public void SetBuffer(int id, byte[] data)
        {
            MyClient client = GetClient(id);
            client.buffer = data;
            _clients[id] = client;
        }

        public void SendSplittedPocket(int id, BasePocket pocket)
        {
            byte[] data = Utils.ConcatBytes(new Header((int)DateTime.Now.Ticks, pocket.GetType(), pocket.ToBytes().Length), pocket);
            int split_count = (data.Length / settings.MaxPocketSize + 1);
            bool first = true;
            do
            {
                byte[] data_part = data;
                if (data.Length > settings.MaxPocketSize)
                    data_part = Utils.SplitBytes(ref data, settings.MaxPocketSize);
                int pocket_enum = (int)PocketEnum.SplittedPocket;
                if (first)
                {
                    pocket_enum = (int)PocketEnum.SplittedPocketStart;
                    first = false;
                }
                if (split_count == 1)
                    pocket_enum = (int)PocketEnum.SplittedPocketEnd;
                Header header = new Header((int)DateTime.Now.Ticks, pocket_enum, data_part.Length);
                data_part = Utils.ConcatBytes(header.ToBytes(), data_part);
                while (!GetClientCallback(id))
                    Thread.Sleep(10);
                SendRawBytes(id, data_part, true);
                split_count--;
                Thread.Sleep(5);
            } while (GetSocket(id) != null && split_count > 0);
        }

        public void SendAccepted(int id, int pocket_id)
        {
            if (ID_list.Contains(id))
                Send(id, new AcceptPocket(pocket_id));
        }

        private void SendTask(int id, byte[] data, int data_id, bool wait_accept)
        {
            while (GetClient(id).socket != null)
            {
                MyClient client = GetClient(id);
                if (client.callback)
                {   
                    client.pocket_id = data_id;
                    client.send_buffer = data;
                    _clients[id] = client;
                    if (wait_accept)
                        UpdateAcceptState(id, false);
                    try
                    { 
                        client.socket.Send(data);
                    }
                    catch
                    {
                        if (settings.ExceptionPrint)
                            Console.WriteLine("[ERROR]: Pocket send to '{0}' failed (pid: {1})", client.name, data_id);
                    }
                    return;
                }
                Thread.Sleep(100);
            }
        }

        public void Send(int id, BasePocket pocket, bool wait_accept = false)
        {
            if (!ID_list.Contains(id))
                return;
            int data_id = (int)DateTime.Now.Ticks;
            byte[] data = pocket.ToBytes();
            byte[] header = new Header(data_id, pocket.GetType(), data.Length).ToBytes();
            data = Utils.ConcatBytes(header, data);
            data = Encryption.Encrypt(data);
            //if (data.Length > _settings.MaxPocketSize)  //For client testing
                //SendSplittedPocket(id, data);
            //else
                (new Task(() => SendTask(id, data, data_id, wait_accept))).Start();
        }

        public void SendRawBytes(int id, byte[] data, bool wait_accept = false)
        {
            if (!ID_list.Contains(id))
                return;
            int data_id = (int)DateTime.Now.Ticks;
            data = Encryption.Encrypt(data);
            // if (data.Length > _settings.MaxPocketSize)  //For client testing
            //SendSplittedPocket(id, data);
            //else
            (new Task(() => SendTask(id, data, data_id, wait_accept))).Start();
        }

        public void SendToAll(BasePocket pocket, bool require_accept = true)
        {
            for (int i = 0; i < GetMaxID(); i++)
                Send(i, pocket, require_accept);
        }

        public void SendToAllExcept(BasePocket pocket, int excepted_id, bool require_accept = true)
        {
            for (int i = 0; i < GetMaxID(); i++)
                if (i != excepted_id)
                    Send(i, pocket, require_accept);
        }

        public void SetRecieve(int id, byte[] data)
        {
            if (!ID_list.Contains(id))
                return;
            MyClient client = GetClient(id);
            client.send_buffer = data;
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
                Thread.Sleep(settings.PingFreq);
                MyClient client = GetClient(id);
                if (client.callback)
                    Send(id, new PingPocket((int)DateTime.Now.Ticks, client.ping));
            } while (GetClient(id).socket != null);
        }

        private void LaunchBackgroundWorkers(int id)
        {
            (new Task(() => ClientPinger(id))).Start();
        }

        public void UpdateClientSocket(int id, Socket new_socket)
        {
            MyClient client = GetClient(id);
            client.socket = new_socket;
            client.send_buffer = null;
            _clients[id] = client;
            LaunchBackgroundWorkers(id);
        }

        public void AddClient(Socket socket, string name)
        {
            int id = GetAvailibleID();
            MyClient newClient = new MyClient
            {
                id = id,
                ip = ((IPEndPoint)socket.RemoteEndPoint).Address.ToString(),
                name = name,
                socket = socket,
                callback = true,
                state = (int)ClientStateEnum.Connected
            };
            _clients.TryAdd(id, newClient);
            ID_list.Add(id);
            LaunchBackgroundWorkers(id);
        }

        public void ReplaceClient(Socket socket, int id)
        {
            _clients.TryGetValue(id, out MyClient client);
            client.socket = socket;
            client.ip = ((IPEndPoint)socket.RemoteEndPoint).Address.ToString();
            _clients[id] = client;
            ToggleConnectionState(id, true);
            LaunchBackgroundWorkers(id);
        }

        public void DeleteClient(int id)
        {
            MyClient client = GetClient(id);
            if (client.timer != null)
            {
                client.timer.Dispose();
                client.timer = null;
            }
            Socket temp_socket = client.socket;
            _history.Add(new Settings.Client { name = client.name, ip = client.ip});
            _clients.TryRemove(id, out _);
            ID_list.Remove(id);
            if (temp_socket != null)
                temp_socket.Disconnect(false);
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

        public ClientStateEnum GetClientState(int id)
        {
            _clients.TryGetValue(id, out MyClient client);
            return (ClientStateEnum)client.state;
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
