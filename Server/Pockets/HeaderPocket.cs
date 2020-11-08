using Server.PocketFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Server
{
    public struct HeaderPocket
    {
        public int Type { get; set; }  // DONT CHANGE
        public int Count { get; set; }
        public static int GetLenght()
        {
            return 8;  // Size of body, needed for right reading from bytes
        }

        public static HeaderPocket FromBytes(byte[] data)
        {
            PocketConstructor pc = new PocketConstructor(data);
            var pocket = new HeaderPocket
            {
                Type = pc.ReadInt32(),
                Count = pc.ReadInt32()
            };
            return pocket;
        }

        public byte[] ToBytes()
        {
            PocketConstructor pc = new PocketConstructor();
            pc.WriteInt32(Type);
            pc.WriteInt32(Count);
            return pc.GetBytes();

        }
    }
}
