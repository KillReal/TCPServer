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

    public enum ResponsePocketEnum: int
    {
        SelectUnit = 20,
        MoveUnit = 21,
        Attack = 22,
        SpawnUnit = 23,
        UpgradeTown = 24,
        Market = 25,
        CaptureMine = 26,
        nextTurn = 27,
    }
}
