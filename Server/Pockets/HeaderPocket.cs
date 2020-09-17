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

        public static int GetLenght()
        {
            return sizeof(int) + sizeof(int);
        }

        public static HeaderPocket FromBytes(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                var br = new BinaryReader(ms);
                var currentObject = new HeaderPocket();

                currentObject.Type = br.ReadInt32();
                currentObject.Count = br.ReadInt32();

                return currentObject;
            }
        }

        public byte[] ToBytes()
        {
            var data = new byte[GetLenght()];

            using (var stream = new MemoryStream(data))
            {
                var writer = new BinaryWriter(stream);
                writer.Write(Type);
                writer.Write(Count);
                return data;
            }

        }
    }
}
