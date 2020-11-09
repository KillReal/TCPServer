using Server.PocketFramework;
using Server.Pockets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Server
{
    public class PingPocket : BasePocket
    {
        public int Tick { get; set; } 
        public int LastPing { get; set; }

        public PingPocket(int tick, int lastping)
        {
            Tick = tick;
            LastPing = lastping;
        }

        public static PingPocket FromBytes(byte[] data)
        {
            PocketConstructor pc = new PocketConstructor(data);
            return new PingPocket(pc.ReadInt32(), pc.ReadInt32());
        }

        public override byte[] ToBytes()
        {
            PocketConstructor pc = new PocketConstructor();
            pc.WriteInt32(Tick);
            pc.WriteInt32(LastPing);
            return pc.GetBytes();
        }

        public static byte[] ConstructSingle(int tick, int lastPing)
        {
            Header header = new Header(PocketEnum.Ping, 1);
            PingPocket ping = new PingPocket(tick, lastPing);
            return Utils.ConcatBytes(header.ToBytes(), ping.ToBytes());
        }
    }
}
