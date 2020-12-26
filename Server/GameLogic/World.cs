using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Server.GameLogic
{
    [Serializable]
    public struct Coord
    {
        public int X;
        public int Y;
        public Coord(int x, int y)
        {
            X = x;
            Y = y;
        }
        public Coord ABS => new Coord(Math.Abs(this.X), Math.Abs(this.Y));
        public static Coord operator -(Coord c1, Coord c2) => new Coord(c1.X - c2.X, c1.Y - c2.Y);
        public static Coord operator +(Coord c1, Coord c2) => new Coord(c1.X + c2.X, c1.Y + c2.Y);
        public static bool operator ==(Coord c1, Coord c2) => c1.X == c2.X && c1.Y == c2.Y;
        public static bool operator ==(Coord me, int c) => me.X == c && me.Y == c;
        public static bool operator !=(Coord c1, Coord c2) => c1.X != c2.X && c1.Y != c2.Y;
        public static bool operator !=(Coord me, int c) => me.X != c && me.Y != c;
        public static bool operator >(Coord c1, Coord c2) => c1.X > c2.X && c1.Y > c2.Y;
        public static bool operator >(Coord me, int c) => me.X > c && me.Y > c;
        public static bool operator <(Coord c1, Coord c2) => c1.X < c2.X && c1.Y < c2.Y;
        public static bool operator <(Coord me, int c) => me.X < c && me.Y < c;
        public static bool operator >=(Coord c1, Coord c2) => c1.X >= c2.X && c1.Y >= c2.Y;
        public static bool operator >=(Coord me, int c) => me.X >= c && me.Y >= c;
        public static bool operator <=(Coord c1, Coord c2) => c1.X <= c2.X && c1.Y <= c2.Y;
        public static bool operator <=(Coord me, int c) => me.X <= c && me.Y <= c;
    }

    [Serializable]
    public class Mine : GameObj
    {
        public enum typeMine
        {
            gold,
            wood,
            rock,
            crystall
        }
        public typeMine TypeMine;
        public Mine(typeMine t)
        {
            type = typeObj.mine;
            TypeMine = t;
        }
        public void mining()
        {
            switch (TypeMine)
            {
                case typeMine.gold:
                    this.owner.gold += 1;
                    break;
                case typeMine.wood:
                    this.owner.wood += 1;
                    break;
                case typeMine.rock:
                    this.owner.rock += 1;
                    break;
                case typeMine.crystall:
                    this.owner.crystall += 1;
                    break;
            }
        }
    }

    [Serializable]
    public class Town : GameObj
    {
        public int level;
        public Town()
        {
            type = typeObj.town;
            level = 1;
        }
        public void upgrade()
        {
            if (level == 3) throw new Exception("Max upgrade");
            level++;
            if (level == 2) health += 2;
            else if (level == 3) health += 3;
        }
        public void nextTurn()
        {
            this.owner.gold += 100 * level;
        }
    }

    [Serializable]
    public class World 
    {
        public GameObj[,] Map;
        public Town[] towns;
        private Coord[,] SpawnPos;

        public void SpawnUnit(Unit u) // OK
        {
            for (int i = 0; i < 3; i++)
            {
                Coord c = SpawnPos[u.owner.id, i];
                if (Map[c.X, c.Y].type == GameObj.typeObj.empty)
                {
                    u.type = GameObj.typeObj.unit;
                    Map[c.X, c.Y] = u;
                    u.Position = new Coord(c.X, c.Y);
                    return;
                }
            }
            throw new Exception("no free places");
        }
    }
}