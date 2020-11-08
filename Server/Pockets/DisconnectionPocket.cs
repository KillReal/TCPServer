using Server.PocketFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Server.Pockets
{
    class DisconnectionPocket : BasePocket
    {
        public string Name { get; set; }
        public string Message { get; set; }

        public DisconnectionPocket()
        {

        }

        public DisconnectionPocket(string name, string message)
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

        public static DisconnectionPocket FromBytes(byte[] data)
        {
            PocketConstructor pc = new PocketConstructor(data);
            return new DisconnectionPocket(pc.ReadString(), pc.ReadString());
        }

        public static byte[] ConstructSingle(string name, string msg)
        {
            Header header = new Header(PocketEnum.Connection, 1);
            DisconnectionPocket connect = new DisconnectionPocket(name, msg);
            return Utils.ConcatBytes(header.ToBytes(), connect.ToBytes());
        }
    }
}
