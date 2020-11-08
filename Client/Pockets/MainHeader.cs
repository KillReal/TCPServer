using Server.PocketFramework;
using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    namespace Server.Pockets
    {
        public class MainHeader
        {
            public int Hash { get; set; } // DONT CHANGE
            public int Id { get; set; }
            public static int GetLenght()
            {
                return 8; // needed for right read from bytes
            }

            public static MainHeader FromBytes(byte[] data)
            {
                PocketConstructor pc = new PocketConstructor(data);
                var pocket = new MainHeader
                {
                    Hash = pc.ReadInt32(),
                    Id = pc.ReadInt32()
                };
                return pocket;
            }

            public byte[] ToBytes()
            {
                PocketConstructor pc = new PocketConstructor();
                pc.WriteInt32(Hash);
                pc.WriteInt32(Id);
                return pc.GetBytes();
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
