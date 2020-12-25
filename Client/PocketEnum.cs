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
        Init = 11,
        PlayState = 12,
        MaxExistEnum = 13    // Need for valid header


        /*  Disconnected = 0,
            Connected = 1,
            Preparing = 2,
            Ready = 3,
            Playing = 4,
            Exiting = 5
        */
    }
}
