using System;
using System.Collections.Generic;
using System.Text;
using Server.GameLogic;
using Server.PocketFramework;

namespace Server.Pockets
{
    class InitGamePocket : BasePocket
    {
        public Player player;

        public static int GetLenght()
        {
            return sizeof(int);
        }

        public InitGamePocket(Player p)
        {
            player = p;
        }

        public override byte[] ToBytes()
        {
            PocketConstructor pc = new PocketConstructor();
            pc.WriteInt32((int)PocketEnum.InitGame);
            pc.WriteInt32(1);
            pc.WriteInt32(player.id);
            // resources player?
            return pc.GetBytes();
        }
    }
}
