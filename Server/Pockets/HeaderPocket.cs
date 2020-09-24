using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Server
{
    public struct HeaderPocket
    {
        public int Type { get; set; }
        public int Count { get; set; }
        public bool NeedAccept { get; set; }
        public static int GetLenght()
        {
            return sizeof(int) + sizeof(int) + sizeof(bool);
        }

        public static HeaderPocket FromBytes(byte[] bytes)
        {
            using var ms = new MemoryStream(bytes);
            var br = new BinaryReader(ms);
            var pocket = new HeaderPocket();
            pocket.Type = br.ReadInt32();
            pocket.Count = br.ReadInt32();
            pocket.NeedAccept = br.ReadBoolean();
            return pocket;
        }

        public byte[] ToBytes()
        {
            var data = new byte[GetLenght()];
            using var stream = new MemoryStream(data);
            var writer = new BinaryWriter(stream);
            writer.Write(Type);
            writer.Write(Count);
            writer.Write(NeedAccept);
            return data;

        }
    }
}
