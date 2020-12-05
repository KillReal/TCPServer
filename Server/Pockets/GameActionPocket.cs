using System;
using Server.GameLogic;
using Server.PocketFramework;
using System.Collections.Generic;
using System.Text;

namespace Server.Pockets
{
    public class GameActionPocket : BasePocket
    {
        public int Buttons { get; set; }
        public int CoordX { get; set; }
        public int CoordY { get; set; }
        public int Param { get; set; }

        public static int GetLenght()
        {
            return 16;
        }

        public GameActionPocket(Game.Buttons _buttons, Coord _coord, int _param)
        {
            Buttons = (int)_buttons;
            CoordX = _coord.X;
            CoordY = _coord.Y;
            Param = _param;
        }

        public static GameActionPocket FromBytes(byte[] data)
        {
            PocketConstructor pc = new PocketConstructor(data);
            return new GameActionPocket((Game.Buttons)pc.ReadInt32(), new Coord(pc.ReadInt32(), pc.ReadInt32()), pc.ReadInt32());
        }

        public override byte[] ToBytes()
        {
            PocketConstructor pc = new PocketConstructor();
            pc.WriteInt32(Buttons);
            pc.WriteInt32(CoordX);
            pc.WriteInt32(CoordY);
            pc.WriteInt32(Param);
            return pc.GetBytes();
        }
    }
}
