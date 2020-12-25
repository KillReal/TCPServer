using Server.PocketFramework;
using Server.Pockets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Server
{
    public class PlayStatePocket : BasePocket
    {
        public int SessionID { get; set; }  // Not used for now
        public int State { get; set; }      // Check ClientStateEnum

        public PlayStatePocket(int session_id, int state)
        {
            SessionID = session_id;
            State = state;
        }

        public PlayStatePocket(int session_id, ClientStateEnum state)
        {
            SessionID = session_id;
            State = (int)state;
        }

        public static PlayStatePocket FromBytes(byte[] data)
        {
            PocketConstructor pc = new PocketConstructor(data);
            return new PlayStatePocket(pc.ReadInt32(), pc.ReadInt32());
        }

        public override byte[] ToBytes()
        {
            PocketConstructor pc = new PocketConstructor();
            pc.WriteInt32(SessionID);
            pc.WriteInt32(State);
            return pc.GetBytes();
        }

        public override int GetType()
        {
            return (int)PocketEnum.PlayState;
        }
    }
}
