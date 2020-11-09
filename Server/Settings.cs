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
        public int ConnectionTimeOut = 10;
        public int ReconnectionTimeOut = 10;
        public int PingTimerFreq = 3;
        public bool ExceptionPrint = false;
        public bool EncryptionEnabled = true;
        public string EncryptionKey = "A24-C356";
        public string EncryptionSalt = "66-BC-GC";
    }
}
