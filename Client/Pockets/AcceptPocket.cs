using Server.PocketFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Server.Pockets
{
    class AcceptPocket : BasePocket
    {
        public int Id { get; set; }

        public AcceptPocket()
        {

        }

        public AcceptPocket(int id)
        {
            Id = id;
        }

        public override byte[] ToBytes()
        {
            PocketConstructor pc = new PocketConstructor();
            pc.WriteInt32(Id);
            return pc.GetBytes();
        }

        public static AcceptPocket FromBytes(byte[] data)
        {
            PocketConstructor pc = new PocketConstructor(data);
            return new AcceptPocket(pc.ReadInt32());
        }

        public override int GetType()
        {
            return (int)PocketEnum.MessageAccepted;
        }
    }
}
