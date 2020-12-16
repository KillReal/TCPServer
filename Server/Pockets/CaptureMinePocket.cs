using System;
using System.Collections.Generic;
using System.Text;
using Server.GameLogic;
using Server.PocketFramework;

namespace Server.Pockets
{
    class CaptureMinePocket : BasePocket // CHANGED!!!
    {
        public Mine mine;

        public CaptureMinePocket(Mine mine)
        {
            this.mine = mine;
        }

        public override byte[] ToBytes()
        {
            PocketConstructor pc = new PocketConstructor();

            pc.WriteInt32(mine.owner.id);
            pc.WriteInt32(mine.Position.X);
            pc.WriteInt32(mine.Position.Y);
            pc.WriteInt32((int)mine.TypeMine);

            return pc.GetBytes();
        }

        public override int GetType()
        {
            return (int)ResponsePocketEnum.CaptureMine;
        }
    }
}
