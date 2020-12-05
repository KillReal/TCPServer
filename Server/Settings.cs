using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class Settings
    {
        public string HostName = "localhost";
        public int Port = 11000;
        public int PocketHash = 332;
        public int MaxClients = 2;
        public int ConnectionTimeOut = 10;   //sec
        public int ReconnectionTimeOut = 10; //sec
        public int SendWaitingFreq = 100;    //ms
        public int PingTimerFreq = 3000;     //ms
        public bool ExceptionPrint = false;
        public bool EncryptionEnabled = true;
        public string EncryptionKey = "A24-C356";
        public string EncryptionSalt = "66-BC-GC";
        public int MaxPocketSize = 20;
    }
}
