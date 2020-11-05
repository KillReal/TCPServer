    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    namespace Server.Pockets
    {
        public class MainHeader
        {
            public int Hash { get; set; }
            public int Id { get; set; }
            public static int GetLenght()
            {
                return sizeof(int) * 2;
            }

            public static MainHeader FromBytes(byte[] bytes)
            {
                using var ms = new MemoryStream(bytes);
                var br = new BinaryReader(ms);
                var pocket = new MainHeader();
                pocket.Hash = br.ReadInt32();
                pocket.Id = br.ReadInt32();
                return pocket;
            }

            public byte[] ToBytes()
            {
                var messageData = new byte[sizeof(int) * 2];
                using var stream = new MemoryStream(messageData);
                var writer = new BinaryWriter(stream);
                writer.Write(Hash);
                writer.Write(Id);
                return messageData;
            }

            public static byte[] Construct(int hash, int id)
            {
                MainHeader header = new MainHeader
                {
                    Hash = hash,
                    Id = id
                };
                return header.ToBytes();
            }
        }
    }
