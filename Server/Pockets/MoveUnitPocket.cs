﻿using System;
using System.Collections.Generic;
using System.Text;
using Server.GameLogic;
using Server.PocketFramework;

namespace Server.Pockets
{
    class MoveUnitPocket : BasePocket
    {
        public Unit unit;
        public Stack<int> path;

        //public static int GetLenght()
        //{
        //    return sizeof(int) + sizeof(int) * path.Length; // not const size
        //}

        public MoveUnitPocket(Unit unit, Stack<int> path)
        {
            this.unit = unit;
            this.path = path;
        }

        public override byte[] ToBytes()
        {
            PocketConstructor pc = new PocketConstructor();
            pc.WriteInt32((int)ResponsePocketEnum.MoveUnit);
            pc.WriteInt32(1);
            pc.WriteInt32(unit.owner.id);
            pc.WriteInt32(path.Count);
            foreach (int item in path)
                pc.WriteInt32(item);
            
            return pc.GetBytes();
        }
    }
}
