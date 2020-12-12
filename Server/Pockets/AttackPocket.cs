using System;
using System.Collections.Generic;
using System.Text;
using Server.GameLogic;
using Server.PocketFramework;

namespace Server.Pockets
{
    class AttackPocket : BasePocket
    {
        public GameObj attacking;
        public GameObj defending;

        public static int GetLenght()
        {
            return sizeof(int) * 6;
        }

        public AttackPocket(GameObj attacking, GameObj defending)
        {
            this.attacking = attacking;
            this.defending = defending;
        }

        public override byte[] ToBytes()
        {
            PocketConstructor pc = new PocketConstructor();
            pc.WriteInt32(attacking.owner.id);
            pc.WriteInt32(attacking.Position.X);
            pc.WriteInt32(attacking.Position.Y);
            pc.WriteInt32(defending.owner.id);
            pc.WriteInt32(defending.Position.X);
            pc.WriteInt32(defending.Position.Y);
            return pc.GetBytes();
        }

        public override int GetType()
        {
            return (int)ResponsePocketEnum.Attack;
        }
    }
}
