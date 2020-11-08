using Server.PocketFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Server.Pockets
{
    class ChatMessagePocket : BasePocket
    {
        public string  Name { get; set; }
        public string Message { get; set; }
        public override byte[] ToBytes()
        {
            PocketConstructor pc = new PocketConstructor();
            pc.WriteString(Name);
            pc.WriteString(Message);
            return pc.GetBytes();
        }

        public static ChatMessagePocket FromBytes(byte[] data)
        {
            PocketConstructor pc = new PocketConstructor(data);
            var pocket = new ChatMessagePocket
            {
                Name = pc.ReadString(),
                Message = pc.ReadString()
            };
            return pocket;
        }

        public static byte[] ConstructSingle(string name, string msg)
        {
            HeaderPocket header = new HeaderPocket
            {
                Count = 1,
                Type = (int)PocketEnum.ChatMessage,
            };
            ChatMessagePocket chat_msg = new ChatMessagePocket
            {
                Name = name,
                Message = msg
            };
            return Utils.ConcatBytes(header.ToBytes(), chat_msg.ToBytes());
        }
    }
}
