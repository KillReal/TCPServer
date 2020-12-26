using System;
using System.Collections.Generic;
using System.Text;
using Server.GameLogic;
using Server.PocketFramework;

namespace Server.Pockets
{
    class MoveUnitPocket : BasePocket // CHANGED!!!
    {
        public Unit unit;
        public Queue<int> path;

        public MoveUnitPocket(Unit unit, Queue<int> path)
        {
            this.unit = unit;
            this.path = path;
        }

        public override byte[] ToBytes()
        {
            PocketConstructor pc = new PocketConstructor();
            pc.WriteInt32(unit.owner.id);
            pc.WriteInt32(unit.Position.X);
            pc.WriteInt32(unit.Position.Y);
            pc.WriteInt32(unit.actionPoints); 
            pc.WriteInt32(path.Count);
            foreach (int item in path)
                pc.WriteInt32(item);
            return pc.GetBytes();
        }

        public override int GetType()
        {
            return (int)ResponsePocketEnum.MoveUnit;
        }
    }
}
