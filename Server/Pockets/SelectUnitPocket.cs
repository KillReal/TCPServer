using System;
using System.Collections.Generic;
using System.Text;
using Server.PocketFramework;
using Server.GameLogic;

namespace Server.Pockets
{
    class SelectUnitPocket : BasePocket
    {
        public Unit unit;

        public static int GetLenght()
        {
            return sizeof(int) * 3;
        }

        public SelectUnitPocket(Unit unit)
        {
            this.unit = unit;
        }

        public override byte[] ToBytes()
        {
            PocketConstructor pc = new PocketConstructor();
            pc.WriteInt32((int)ResponsePocketEnum.SelectUnit);
            pc.WriteInt32(unit.owner.id);
            pc.WriteInt32(unit.Position.X);
            pc.WriteInt32(unit.Position.Y);
            return pc.GetBytes();
        }
    }
}
