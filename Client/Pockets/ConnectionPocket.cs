using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Server.Pockets
{
    class ConnectionPocket : BasePocket
    {
        private int NameLenght { get; set; }
        public string Name { get; set; }
        private int MessageLenght { get; set; }
        public string Message { get; set; }
        public override byte[] ToBytes()
        {
            byte[] nameBytes = Utils.GetBytes(Name);
            NameLenght = nameBytes.Length;
            byte[] messageBytes = Utils.GetBytes(Message);
            MessageLenght = messageBytes.Length;
            int messageLenght = sizeof(int) * 2 + NameLenght + MessageLenght;
            var messageData = new byte[messageLenght];
            using (var stream = new MemoryStream(messageData))
            {
                var writer = new BinaryWriter(stream);
                writer.Write(NameLenght);
                writer.Write(nameBytes);
                writer.Write(MessageLenght);
                writer.Write(messageBytes);
                return messageData;
            }
        }

        public static ConnectionPocket FromBytes(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                var br = new BinaryReader(ms);
                var pocket = new ConnectionPocket();

                pocket.NameLenght = br.ReadInt32();
                pocket.Name = Utils.GetString(br.ReadBytes(pocket.NameLenght));

                pocket.MessageLenght = br.ReadInt32();
                pocket.Message = Utils.GetString(br.ReadBytes(pocket.MessageLenght));

                return pocket;
            }
        }
    }
}
