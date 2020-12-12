using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Pockets
{
    public abstract class BasePocket
    {
        public abstract byte[] ToBytes();

        public abstract int GetType();
    }
}
