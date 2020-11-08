using Server.PocketFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Server.Pockets
{
    public class MainHeader : BasePocket
    {
        public int Hash { get; set; } // DONT CHANGE
        public int Id { get; set; }
        public static int GetLenght()
        {
            return 8; // needed for right read from bytes
        }

        public MainHeader(int hash, int id)
        {
            Hash = hash;
            Id = id;
        }

        public static MainHeader FromBytes(byte[] data)
        {
            PocketConstructor pc = new PocketConstructor(data);
            return new MainHeader(pc.ReadInt32(), pc.ReadInt32());
        }

        public override byte[] ToBytes()
        {
            PocketConstructor pc = new PocketConstructor();
            pc.WriteInt32(Hash);
            pc.WriteInt32(Id);
            return pc.GetBytes();
        }
    }
}
