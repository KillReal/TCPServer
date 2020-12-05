using System;
using Server.GameLogic;
using Server.PocketFramework;
using System.Collections.Generic;
using System.Text;

namespace Server.Pockets
{
    public class GameActionPocket : BasePocket
    {
        public static Game.Buttons Buttons { get; set; }
        public static Coord Coord { get; set; }
        public static int Param { get; set; }

        public static int GetLenght()
        {
            return 16;
        }

        public GameActionPocket(Game.Buttons _buttons, Coord _coord, int _param)
        {
            Buttons = _buttons;
            Coord = _coord;
            Param = _param;
        }

        public static GameActionPocket FromBytes(byte[] data)
        {
            PocketConstructor pc = new PocketConstructor(data);
            Buttons = (Game.Buttons)pc.ReadInt32();
            Coord = new Coord(pc.ReadInt32(), pc.ReadInt32());
            Param = pc.ReadInt32();
            return new GameActionPocket(Buttons, Coord, Param);
        }

        public override byte[] ToBytes()
        {
            PocketConstructor pc = new PocketConstructor();
            pc.WriteInt32((int)Buttons);
            pc.WriteInt32(Coord.X);
            pc.WriteInt32(Coord.Y);
            pc.WriteInt32(Param);
            return pc.GetBytes();
        }
    }
}
