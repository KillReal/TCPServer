using System;
using System.Collections.Generic;
using System.Text;
using Server.GameLogic;
using Server.PocketFramework;

namespace Server.Pockets
{
    class PlayerResourcesPocket: BasePocket
    {
        public Player player;

        public PlayerResourcesPocket(Player player)
        {
            this.player = player;
        }

        public override byte[] ToBytes()
        {
            PocketConstructor pc = new PocketConstructor();

            pc.WriteInt32(player.id);
            pc.WriteInt32(player.gold);
            pc.WriteInt32(player.wood);
            pc.WriteInt32(player.rock);
            pc.WriteInt32(player.crystall);

            return pc.GetBytes();
        }

        public override int GetType()
        {
            return (int)ResponsePocketEnum.Resources;
        }
    }
}
