using System;
using System.Collections.Generic;
using System.Text;
using Server.GameLogic;
using Server.PocketFramework;

namespace Server.Pockets
{
    class ReadyToGamePocket : BasePocket
    {
        public ReadyToGamePocket()
        {
        }

        public override byte[] ToBytes()
        {
            PocketConstructor pc = new PocketConstructor();
            return pc.GetBytes();
        }

        public override int GetType()
        {
            return (int)ResponsePocketEnum.READY;
        }
    }
}
