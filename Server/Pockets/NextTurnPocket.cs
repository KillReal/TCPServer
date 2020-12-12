using System;
using System.Collections.Generic;
using System.Text;
using Server.GameLogic;
using Server.PocketFramework;

namespace Server.Pockets
{
    class NextTurnPocket : BasePocket
    {
        public Player player;

        public static int GetLenght()
        {
            return sizeof(int);
        }

        public NextTurnPocket(Player player)
        {
            this.player = player;
        }

        public override byte[] ToBytes()
        {
            PocketConstructor pc = new PocketConstructor();
            pc.WriteInt32((int)ResponsePocketEnum.nextTurn);
            pc.WriteInt32(1);
            pc.WriteInt32(player.id);
            return pc.GetBytes();
        }
    }
}
