using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Server
{
    public class Settings
    {
        public string HostName = "localhost";
        public int Port = 11000;
        public int MaxClients = 4;
        public int MaxSessionClients = 2;
        public int ConnectionTimeOut = 10;   //sec
        public int ReconnectionTimeOut = 10; //sec
        public int SendWaitingFreq = 100;    //ms
        public int PingFreq = 3000;     //ms
        public bool ExceptionPrint = true;
        public bool EncryptionEnabled = false;
        public string EncryptionKey = "A24-C356A24-C356";
        public string EncryptionSalt = "66-BC-GC66-BC-GC";
        public int MaxPocketSize = 20;
        public string BannedClientsPath = "./banned.xml";
        public string ConfigPath = "./config.xml";

        public struct Options
        {
            public string HostName;
            public int Port;
            public int MaxClients;
            public int MaxSessionClients;
            public int ConnectionTimeOut;   //sec
            public int ReconnectionTimeOut; //sec
            public int SendWaitingFreq;    //ms
            public int PingTimerFreq;     //ms
            public bool ExceptionPrint;
            public bool EncryptionEnabled;
            public string EncryptionKey;
            public int MaxPocketSize;
            public string BannedClientsPath;
        }

        public struct Client
        {
            public string name;
            public string ip;
        }

        public List<Client> bannedClients = new List<Client>();

        public void Init()
        {
            Console.WriteLine("[INFO]:  Loading settings...");
            bannedClients = DeSerializeObject<List<Client>>(BannedClientsPath);
            Options options = DeSerializeObject<Options>(ConfigPath);
            if (options.HostName != null)
            {
                HostName = options.HostName;
                Port = options.Port;
                MaxClients = options.MaxClients;
                MaxSessionClients = options.MaxSessionClients;
                ConnectionTimeOut = options.ConnectionTimeOut;   //sec
                ReconnectionTimeOut = options.ReconnectionTimeOut; //sec
                SendWaitingFreq = options.SendWaitingFreq;    //ms
                PingFreq = options.PingTimerFreq;     //ms
                ExceptionPrint = options.ExceptionPrint;
                EncryptionEnabled = options.EncryptionEnabled;
                EncryptionKey = options.EncryptionKey;
                MaxPocketSize = options.MaxPocketSize;
                BannedClientsPath = options.BannedClientsPath;
            }
            if (bannedClients == null)
                bannedClients = new List<Client>();
        }

        public void SaveChanges()
        {
            Console.WriteLine("[INFO]:  Saving settings...");
            if (!File.Exists(BannedClientsPath))
                File.Create(BannedClientsPath);
            SerializeObject(bannedClients, BannedClientsPath);
            if (!File.Exists(ConfigPath))
                File.Create(ConfigPath);
            Options options = new Options
            {
                HostName = HostName,
                Port = Port,
                MaxClients = MaxClients,
                MaxSessionClients = MaxSessionClients,
                ConnectionTimeOut = ConnectionTimeOut,   //sec
                ReconnectionTimeOut = ReconnectionTimeOut, //sec
                SendWaitingFreq = SendWaitingFreq,    //ms
                PingTimerFreq = PingFreq,     //ms
                ExceptionPrint = ExceptionPrint,
                EncryptionEnabled = EncryptionEnabled,
                EncryptionKey = EncryptionKey,
                MaxPocketSize = MaxPocketSize,
                BannedClientsPath = BannedClientsPath
            };
            SerializeObject(options, ConfigPath);
        }

        public void AddBannedClient(string _name, string _ip)
        {
            if (bannedClients.Contains(new Client { name = _name, ip = _ip}))
            {
                Console.WriteLine($"[SERVER]:  '{_name}' is already banned");
                return;
            }
            bannedClients.Add(new Client { name = _name, ip = _ip });
            Console.WriteLine($"[SERVER]:  '{_name}' was banned by ip: {_ip}");
        }

        public void DeleteBannedClient(int id)
        {
            Console.WriteLine($"[SERVER]:  '{bannedClients[id].name}' was unbanned");
            bannedClients.RemoveAt(id);
        }

        public bool CheckClientBan(string _name, string _ip)
        {
            for (int i = 0; i < bannedClients.Count; i++)
            {
                if (bannedClients[i].name == _name)
                //if (bannedClients[i].name == _name || bannedClients[i].ip == _ip)
                    return true;
            }
            return false;
        }

        public void SerializeObject<T>(T serializableObject, string fileName)
        {
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                XmlSerializer serializer = new XmlSerializer(serializableObject.GetType());
                using (MemoryStream stream = new MemoryStream())
                {
                    serializer.Serialize(stream, serializableObject);
                    stream.Position = 0;
                    xmlDocument.Load(stream);
                    xmlDocument.Save(fileName);
                }
                Console.WriteLine($"[INFO]:  Successfully saved file {fileName}");
            }
            catch (Exception ex)
            {
                if (ExceptionPrint)
                    Console.WriteLine($"[ERROR]:  Saving file {fileName} is failed ({ex.Message})");
            }
        }


        public T DeSerializeObject<T>(string fileName)
        {
            T objectOut = default(T);
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(fileName);
                string xmlString = xmlDocument.OuterXml;
                using (StringReader read = new StringReader(xmlString))
                {
                    Type outType = typeof(T);
                    XmlSerializer serializer = new XmlSerializer(outType);
                    using (XmlReader reader = new XmlTextReader(read))
                        objectOut = (T)serializer.Deserialize(reader);
                }
                Console.WriteLine($"[INFO]:  Successfully loaded file {fileName}");
            }
            catch (Exception ex)
            {
                if (ExceptionPrint)
                    Console.WriteLine($"[ERROR]:  Loading file {fileName} is failed ({ex.Message})");
            }
            return objectOut;
        }
    }
}
