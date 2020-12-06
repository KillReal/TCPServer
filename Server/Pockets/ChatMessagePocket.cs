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

        public ChatMessagePocket()
        {

        }

        public ChatMessagePocket(string name, string message)
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

        public static ChatMessagePocket FromBytes(byte[] data)
        {
            PocketConstructor pc = new PocketConstructor(data);
            return new ChatMessagePocket(pc.ReadString(), pc.ReadString());
        }

        public static byte[] ConstructSingle(string name, string msg)
        {
            Header header = new Header(PocketEnum.ChatMessage, 1);
            ChatMessagePocket chat_msg = new ChatMessagePocket(name, msg);
            return Utils.ConcatBytes(header, chat_msg);
        }
    }
}
