using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Server
{
    [Serializable]
    public class Options
    {
        public string HostName;
        public int Port;
        public int MaxClients;
        public int MaxSessionClients;
        public int ConnectionTimeOut;   //sec
        public int ReconnectionTimeOut; //sec
        public int SendWaitingFreq;    //ms
        public int PingFreq;     //ms
        public bool EncryptionEnabled;
        public string EncryptionKey;
        public int MaxPocketSize;
        public int DebugPrintLevel = 2;

        public void SetDefault()
        {
            HostName = "localhost";
            Port = 11000;
            MaxClients = 4;
            MaxSessionClients = 2;
            ConnectionTimeOut = 10;   //sec
            ReconnectionTimeOut = 10; //sec
            SendWaitingFreq = 100;    //ms
            PingFreq = 3000;     //ms
            DebugPrintLevel = 2;
            EncryptionEnabled = false;
            EncryptionKey = "A24-C356A24-C356";
            MaxPocketSize = 20;
        }
    }
}
