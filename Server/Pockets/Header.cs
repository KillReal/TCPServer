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
        public int Id { get; set; }
        public int Type { get; set; }  
        public int Size { get; set; }
        public static int GetLenght()
        {
            return 12;  // Size of body, needed for right reading from bytes
        }

        public Header(int id, int type, int size)
        {
            Type = type;
            Size = size;
            Id = id;
        }

        public Header(PocketEnum type, int size, int id)
        {
            Type = (int)type;
            Size = size;
            Id = id;
        }

        public static Header FromBytes(byte[] data)
        {
            PocketConstructor pc = new PocketConstructor(data);
            return new Header(pc.ReadInt32(), pc.ReadInt32(), pc.ReadInt32());
        }

        public override byte[] ToBytes()
        {
            PocketConstructor pc = new PocketConstructor();
            pc.WriteInt32(Id);
            pc.WriteInt32(Type);
            pc.WriteInt32(Size);
            return pc.GetBytes();
        }

        public override int GetType()
        {
            return -1;
        }
    }
}
