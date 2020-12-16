using System;
using System.Collections.Generic;
using System.Text;
using Server.PocketFramework;
using Server.GameLogic;

namespace Server.Pockets
{
    class EndGamePocket : BasePocket
    {
        public Player player;
        public int place;

        public EndGamePocket(Player player, int place)
        {
            this.player = player;
            this.place = place;
        }

        public override byte[] ToBytes()
        {
            PocketConstructor pc = new PocketConstructor();
            pc.WriteInt32(player.id);
            pc.WriteInt32(place);
            return pc.GetBytes();
        }

        public override int GetType()
        {
            return (int)PocketEnum.EndGame;
        }
    }
}
