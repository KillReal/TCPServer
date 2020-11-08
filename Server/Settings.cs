﻿using System;
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
        public int ReconnectionTimeOut = 30;
        public bool ExceptionPrint = false;
    }
}
