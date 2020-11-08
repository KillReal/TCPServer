using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public enum PocketEnum : int
    {
        Connection = 1,
        Disconnection = 2,
        MessageAccepted = 3,
        ChatMessage = 4,
        ServerInfo = 5
    }
}
