using Server.Pockets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Server.PocketFramework
{
    public class PocketConstructor
    {
        private byte[] Data { get; set; }

        public PocketConstructor()
        {
            Data = new byte[] { };
        }

        public PocketConstructor(byte[] data)
        {
            Data = data;
        }

        public void WriteInt32(int val)
        {
            Data = Utils.ConcatBytes(Data, Utils.IntToBytes(val));
        }

        public int ReadInt32()
        {
            byte[] bytes = new byte[4];
            byte[] newData = new byte[Data.Length - 4];
            Buffer.BlockCopy(Data, 0, bytes, 0, 4);
            Buffer.BlockCopy(Data, 4, newData, 0, Data.Length - 4);
            Data = newData;
            return Utils.BytesToInt(bytes);
        }

        public void WriteString(string str)
        {
            WriteInt32(str.Length * 2);
            byte[] bytes = Utils.StrToBytes(str);
            Data = Utils.ConcatBytes(Data, bytes);
        }

        public string ReadString()
        {
            int len = ReadInt32();
            byte[] bytes = new byte[len];
            byte[] newData = new byte[Data.Length - len];
            Buffer.BlockCopy(Data, 0, bytes, 0, len);
            Buffer.BlockCopy(Data, len, newData, 0, Data.Length - len);
            Data = newData;
            return Utils.BytesToStr(bytes);
        }
        
        public byte[] GetBytes()
        {
            return Data;
        }
    }
}
