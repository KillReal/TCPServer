using Server.PocketFramework;
using Server.Pockets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Server
{
    public class Header : BasePocket
    {
        public int Type { get; set; }  // DONT CHANGE
        public int Count { get; set; }
        public static int GetLenght()
        {
            return 8;  // Size of body, needed for right reading from bytes
        }

        public Header(int type, int count)
        {
            Type = type;
            Count = count;
        }

        public Header(PocketEnum type, int count)
        {
            Type = (int)type;
            Count = count;
        }

        public static Header FromBytes(byte[] data)
        {
            PocketConstructor pc = new PocketConstructor(data);
            return new Header(pc.ReadInt32(), pc.ReadInt32());
        }

        public override byte[] ToBytes()
        {
            PocketConstructor pc = new PocketConstructor();
            pc.WriteInt32(Type);
            pc.WriteInt32(Count);
            return pc.GetBytes();
        }
    }
}
