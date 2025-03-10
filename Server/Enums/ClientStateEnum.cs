﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public enum ClientStateEnum : int
    {
        Disconnected    = 0,
        Connected       = 1,
        Preparing       = 2,
        Ready           = 3,
        Playing         = 4,
        Exiting         = 5
    }
}
