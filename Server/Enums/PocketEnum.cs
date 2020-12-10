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
        SplittedPocket = 6,
        GameAction = 7,
        ErrorPocket = 8,
    }

    public enum ResponsePocketEnum: int
    {
        SelectUnit = 10,
        MoveUnit = 11,
        Attack = 12,
        SpawnUnit = 13,
        UpgradeTown = 14,
        Market = 15,
        CaptureMine = 16,
        nextTurn = 17,

    }
}
