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
        public static bool operator >(Coord me, double c) => Math.Sqrt(Math.Pow(me.X, 2) + Math.Pow(me.Y, 2)) > c;
        public static bool operator <(Coord c1, Coord c2) => c1.X < c2.X && c1.Y < c2.Y;
        public static bool operator <(Coord me, double c) => Math.Sqrt(Math.Pow(me.X, 2) + Math.Pow(me.Y, 2)) < c;
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
                    this.owner.gold += 100;
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
            level = 0;
            health = 3;
        }
        public void upgrade()
        {
            switch (level)
            {
                case 0:
                    if (owner.gold - 200 < 0) throw new Exception("Not gold");
                    if (owner.wood - 1 < 0) throw new Exception("Not wood");

                    owner.gold -= 200;
                    owner.wood -= 1;

                    health += 5;
                    break;
                case 1:
                    if (owner.gold - 300 < 0) throw new Exception("Not gold");
                    if (owner.wood - 2 < 0) throw new Exception("Not wood");
                    if (owner.rock - 1 < 0) throw new Exception("Not rock");

                    owner.gold -= 300;
                    owner.wood -= 2;
                    owner.rock -= 1;

                    health += 5;
                    break;
                case 2:
                    if (owner.gold - 400 < 0) throw new Exception("Not gold");
                    if (owner.wood - 3 < 0) throw new Exception("Not wood");
                    if (owner.rock - 2 < 0) throw new Exception("Not rock");
                    if (owner.crystall - 1 < 0) throw new Exception("Not crystall");

                    owner.gold -= 400;
                    owner.wood -= 3;
                    owner.rock -= 2;
                    owner.crystall -= 1;

                    health += 10;
                    break;
                default:
                    throw new Exception("Max upgrade");
                    break;
            }
            level++;
        }
        public void nextTurn()
        {
            this.owner.gold += 100 * level + 50;
        }
    }

    [Serializable]
    public class World 
    {
        public GameObj[,] Map;
        public Town[] towns;
        private Coord[,] SpawnPos;

        public Coord SpawnUnit(Player p) // OK
        {
            for (int i = 0; i < 3; i++)
            {
                Coord c = SpawnPos[p.id, i];
                if (Map[c.X, c.Y].type == GameObj.typeObj.empty)
                {
                    //u.type = GameObj.typeObj.unit;
                    //Map[c.X, c.Y] = u;
                    //u.Position = new Coord(c.X, c.Y);
                    return c;
                }
            }
            throw new Exception("no free places");
        }
    }
}