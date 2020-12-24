using System;
using System.Collections.Generic;
using System.Text;
using Server.GameLogic;
using Server.PocketFramework;

namespace Server.Pockets
{
    class SpawnUnitPocket : BasePocket
    {
        public Unit unit;

        public static int GetLenght()
        {
            return sizeof(int) * 4;
        }

        public SpawnUnitPocket(Unit unit)
        {
            this.unit = unit;
        }

        public override byte[] ToBytes()
        {
            PocketConstructor pc = new PocketConstructor();
            pc.WriteInt32(unit.owner.id);
            pc.WriteInt32(unit.Position.X);
            pc.WriteInt32(unit.Position.Y);
            pc.WriteInt32((int)unit.type_unit);

            pc.WriteInt32(unit.attack);
            pc.WriteInt32(unit.defense);
            pc.WriteInt32(unit.damage);
            pc.WriteInt32(unit.health);
            pc.WriteInt32(unit.MAXactionPoints);
            pc.WriteInt32(unit.rangeAttack);
            pc.WriteInt32(unit.shootingDamage);
            return pc.GetBytes();
        }

        public override int GetType()
        {
            return (int)ResponsePocketEnum.SpawnUnit;
        }
    }
}
