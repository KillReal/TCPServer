using Server.PocketFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Server.Pockets
{
    class ConnectionPocket : BasePocket
    {
        public string Name { get; set; }
        public string Message { get; set; }
        public override byte[] ToBytes()
        {
            PocketConstructor pc = new PocketConstructor();
            pc.WriteString(Name);
            pc.WriteString(Message);
            return pc.GetBytes();
        }

        public static ConnectionPocket FromBytes(byte[] data)
        {
            PocketConstructor pc = new PocketConstructor(data);
            var pocket = new ConnectionPocket
            {
                Name = pc.ReadString(),
                Message = pc.ReadString()
            };
            return pocket;
        }
    }
}
