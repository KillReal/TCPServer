using System;
using Server.GameLogic;
using Server.PocketFramework;
using System.Collections.Generic;
using System.Text;

namespace Server.Pockets
{
    public class GameActionPocket : BasePocket
    {
        public GameManager.Buttons Button;
        //public int countParams;
        //public int[] parameters;
        public Coord Coord { get; set; }
        public int Param { get; set; }

        public static int GetLenght()
        {
            return sizeof(int)*4;
        }

        public GameActionPocket(GameManager.Buttons _button, Coord _coord, int _param)
        {
            Button = _button;
            Coord = _coord;
            Param = _param;
        }

        public static GameActionPocket FromBytes(byte[] data)
        {
            PocketConstructor pc = new PocketConstructor(data);
            return new GameActionPocket((GameManager.Buttons)pc.ReadInt32(), new Coord(pc.ReadInt32(), pc.ReadInt32()), pc.ReadInt32());
        }

        public override byte[] ToBytes()
        {
            PocketConstructor pc = new PocketConstructor();
            pc.WriteInt32((int)Button);
            pc.WriteInt32(Coord.X);
            pc.WriteInt32(Coord.Y);
            pc.WriteInt32(Param);
            return pc.GetBytes();
        }

        public override int GetType()
        {
            return (int)PocketEnum.GameAction;
        }
    }
}
