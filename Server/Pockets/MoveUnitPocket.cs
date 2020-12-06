using System;
using System.Collections.Generic;
using System.Text;
using Server.GameLogic;
using Server.PocketFramework;

namespace Server.Pockets
{
    class MoveUnitPocket : BasePocket
    {
        public Unit unit;
        public int[] path;

        //public static int GetLenght()
        //{
        //    return sizeof(int) + sizeof(int) * path.Length; // not const size
        //}

        public MoveUnitPocket(Unit unit, int[] path)
        {
            this.unit = unit;
            this.path = path;
        }

        public override byte[] ToBytes()
        {
            PocketConstructor pc = new PocketConstructor();
            pc.WriteInt32((int)ResponsePocketEnum.MoveUnit);
            pc.WriteInt32(unit.owner.id);
            pc.WriteInt32(path.Length);
            for (int i = 0; i < path.Length; i++)
                pc.WriteInt32(path[i]);            
            return pc.GetBytes();
        }
    }
}
