using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Text;
using System.Threading;

namespace Server
{
    public static class DataManager
    {
        static private Options options;
        static private string LogFilePath = "./log.txt";
        static private string BannedClientsPath = "./banned.xml";
        static private string ConfigPath = "./config.xml";

        public struct Client
        {
            public string name;
            public string ip;
        }
        public static List<Client> bannedClients = new List<Client>();

        public static Options Init(Options _options)
        {
            options = _options;
            LogLine("[INFO]:  Loading settings...");
            bannedClients = DeSerializeObject<List<Client>>(BannedClientsPath);
            if (bannedClients == null)
                bannedClients = new List<Client>();
            options = DeSerializeObject<Options>(ConfigPath);
            if (options == null)
            {
                options = new Options();
                options.SetDefault();
                //File.Delete(ConfigPath);
            }
            LogLine($"------------------------------------------\n[DATE]: {DateTime.Now}", 3);
            return options;
        }

        public static void SaveChanges()
        {
            LogLine("[INFO]:  Saving settings...");
            if (!File.Exists(BannedClientsPath))
                File.Create(BannedClientsPath);
            SerializeObject(bannedClients, BannedClientsPath);
            if (!File.Exists(ConfigPath))
                File.Create(ConfigPath);
            SerializeObject(options, ConfigPath);
        }

        public static void AddBannedClient(string _name, string _ip)
        {
            if (bannedClients.Contains(new Client { name = _name, ip = _ip }))
            {
                LogLine($"[SERVER]:  '{_name}' is already banned", 3);
                return;
            }
            bannedClients.Add(new Client { name = _name, ip = _ip });
            LogLine($"[SERVER]:  '{_name}' was banned by ip: {_ip}");
        }

        public static void DeleteBannedClient(int id)
        {
            LogLine($"[SERVER]:  '{bannedClients[id].name}' was unbanned");
            bannedClients.RemoveAt(id);
        }

        public static bool CheckClientBan(string _name, string _ip)
        {
            for (int i = 0; i < bannedClients.Count; i++)
            {
                if (bannedClients[i].name == _name)
                    //if (bannedClients[i].name == _name || bannedClients[i].ip == _ip)
                    return true;
            }
            return false;
        }

        public static void LogLine(string message, int level = 0)
        {
            if (options.DebugPrintLevel >= level)
                Console.WriteLine(message);
            try
            {
                using (StreamWriter sw = File.AppendText(LogFilePath))
                    sw.WriteLine(message);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"[ERROR]: Failed write to log file ({exception.Message})");
            }
        }
        public static void Log(string message, int level = 0)
        {
            if (options.DebugPrintLevel >= level)
                Console.Write(message);
            try
            {
                using (StreamWriter sw = File.AppendText(LogFilePath))
                    sw.Write(message);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"[ERROR]: Failed write to log file ({exception.Message})");
            }
        }

        public static void SerializeObject<T>(T serializableObject, string fileName)
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
                LogLine($"[INFO]:  Successfully saved file {fileName}", 1);
            }
            catch (Exception ex)
            {
                LogLine($"[ERROR]:  Saving file {fileName} is failed ({ex.Message})", 2);
            }
        }

        public static T DeSerializeObject<T>(string fileName)
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
                LogLine($"[INFO]:  Successfully loaded file {fileName}", 1);
            }
            catch (Exception ex)
            {
                LogLine($"[ERROR]:  Loading file {fileName} is failed ({ex.Message})", 2);
            }
            return objectOut;
        }
    }
}
