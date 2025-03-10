﻿using System;
using System.Collections.Generic;
using System.Text;
using Server.GameLogic;
using Server.PocketFramework;

namespace Server.Pockets
{
    class NextTurnPocket : BasePocket // CHANGED!!!
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

            pc.WriteInt32(player.id);
            //pc.WriteInt32(player.selectUnit); 
            return pc.GetBytes();
        }

        public override int GetType()
        {
            return (int)ResponsePocketEnum.nextTurn;
        }
    }
}
