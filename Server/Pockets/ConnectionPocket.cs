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

        public ConnectionPocket()
        {

        }

        public ConnectionPocket(string name, string message)
        {
            Name = name;
            Message = message;
        }

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
            return new ConnectionPocket(pc.ReadString(), pc.ReadString());
        }

        public static byte[] ConstructSingle(string name, string msg)
        {
            Header header = new Header(PocketEnum.Connection, 1);
            ConnectionPocket connect = new ConnectionPocket(name, msg);
            return Utils.ConcatBytes(header, connect);
        }
    }
}
