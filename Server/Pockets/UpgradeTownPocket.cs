using System;
using System.Collections.Generic;
using System.Text;
using Server.GameLogic;
using Server.PocketFramework;

namespace Server.Pockets
{

    class UpgradeTownPocket : BasePocket
    {
        public Town town;

        public static int GetLenght()
        {
            return sizeof(int) * 3;
        }

        public UpgradeTownPocket(Town town)
        {
            this.town = town;
        }

        public override byte[] ToBytes()
        {
            PocketConstructor pc = new PocketConstructor();
            pc.WriteInt32((int)ResponsePocketEnum.UpgradeTown);
            pc.WriteInt32(town.owner.id);
            // coord ?
            pc.WriteInt32(town.level);
            pc.WriteInt32(town.health);
            return pc.GetBytes();
        }
    }
}
