using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Enums
{
    public enum ClientStateEnum : int
    {
        Idle = 1,
        AskedForAccept = 2,
        WaitForAccept = 3
    }
}
