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
        Ping = 5,
        SplittedPocketStart = 6,
        SplittedPocket = 7,
        SplittedPocketEnd = 8,
        GameAction = 9,
        Error = 10,
        Init = 11
    }
}
