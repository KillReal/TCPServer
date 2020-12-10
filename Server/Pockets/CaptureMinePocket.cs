using System;
using System.Collections.Generic;
using System.Text;
using Server.GameLogic;
using Server.PocketFramework;

namespace Server.Pockets
{
    class CaptureMinePocket : BasePocket
    {
        public Mine mine;

        public static int GetLenght()
        {
            return sizeof(int) * 3;
        }

        public CaptureMinePocket(Mine mine)
        {
            this.mine = mine;
        }

        public override byte[] ToBytes()
        {
            PocketConstructor pc = new PocketConstructor();
            pc.WriteInt32((int)ResponsePocketEnum.CaptureMine);
            pc.WriteInt32(1);
            pc.WriteInt32(mine.owner.id);
            pc.WriteInt32(mine.Position.X);
            pc.WriteInt32(mine.Position.Y);
            return pc.GetBytes();
        }
    }
}
