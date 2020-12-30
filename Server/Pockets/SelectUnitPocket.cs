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
            pc.WriteInt32(unit.owner.id);
            pc.WriteInt32(unit.Position.X);
            pc.WriteInt32(unit.Position.Y);
            pc.WriteInt32(unit.actionPoints);
            return pc.GetBytes();
        }

        public override int GetType()
        {
            return (int)ResponsePocketEnum.SelectUnit;
        }
    }
}
