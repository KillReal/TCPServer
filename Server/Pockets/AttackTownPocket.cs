using System;
using System.Collections.Generic;
using System.Text;
using Server.GameLogic;
using Server.PocketFramework;

namespace Server.Pockets
{
    class AttackTownPocket : BasePocket
    {
        public Unit unit;
        public Town town;

        public AttackTownPocket(Unit unit, Town town)
        {
            this.unit = unit;
            this.town = town;
        }

        public override byte[] ToBytes()
        {
            PocketConstructor pc = new PocketConstructor();

            pc.WriteInt32(unit.owner?.id ?? -1);
            pc.WriteInt32(unit.Position.X);
            pc.WriteInt32(unit.Position.Y);
            pc.WriteInt32(unit.health);

            pc.WriteInt32(town.owner?.id ?? -1);
            pc.WriteInt32(town.Position.X);
            pc.WriteInt32(town.Position.Y);
            pc.WriteInt32(town.health);

            return pc.GetBytes();
        }

        public override int GetType()
        {
            return (int)ResponsePocketEnum.AttackTown;
        }
    }
}
