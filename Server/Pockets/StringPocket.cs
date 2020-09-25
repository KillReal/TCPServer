using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Server.Pockets
{
    class StringPocket : BasePocket
    {
        private int StringFieldLenght { get; set; }
        public string StringField { get; set; }

        public override byte[] ToBytes()
        {
            byte[] stringFieldBytes = Utils.GetBytes(StringField);
            StringFieldLenght = stringFieldBytes.Length;
            int messageLenght = sizeof(int) + StringFieldLenght;
            var messageData = new byte[messageLenght];
            using var stream = new MemoryStream(messageData);
            var writer = new BinaryWriter(stream);
            writer.Write(StringFieldLenght);
            writer.Write(stringFieldBytes);
            return messageData;
        }

        public static StringPocket FromBytes(byte[] bytes)
        {
            using var ms = new MemoryStream(bytes);
            var br = new BinaryReader(ms);
            var pocket = new StringPocket();
            pocket.StringFieldLenght = br.ReadInt32();
            pocket.StringField = Utils.GetString(br.ReadBytes(pocket.StringFieldLenght));
            return pocket;
        }
    }
}
