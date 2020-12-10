using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Server.GameLogic
{
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
        public static Coord operator -(Coord c1, Coord c2)
        {
            return new Coord { X = c1.X - c2.X, Y = c1.Y - c2.Y };
        }
        public static bool operator ==(Coord c1, Coord c2)
        {
            return c1.X == c2.X && c1.Y == c2.Y;
        }
        public static bool operator !=(Coord c1, Coord c2)
        {
            return c1.X != c2.X && c1.Y != c2.Y;
        }
    }

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

    public class Town : GameObj
    {
        public int level;
        public void upgrade()
        {
            if (level == 3) throw new Exception("Max upgrade");
            level++;
            if (level == 2) health += 2;
            else if (level == 3) health += 3;
        }
    }

    public class _Map // need FIX
    {
        public GameObj[,] Map;
        public Coord[,] SpawnPos;
        public List<Town> Towns;
        public List<Mine> Mines;
        public _Map(string fileMap)
        {
            using (StreamReader fs = new StreamReader(fileMap))
            {
                string[] s = fs.ReadLine().Split(' ');
                Map = new GameObj[int.Parse(s[1]), int.Parse(s[0])];
                Mines = new List<Mine>();
                Towns = new List<Town>();
                for (int y = 0; y < Map.GetUpperBound(1) + 1; y++) // 51
                {
                    s = fs.ReadLine().Split(' ');
                    for (int x = 0; x < Map.GetUpperBound(0) + 1; x++) // 71
                    {
                        switch ((GameObj.typeObj)int.Parse(s[x]))
                        {
                            case GameObj.typeObj.empty:
                            case GameObj.typeObj.block:
                                Map[x, y] = new GameObj();
                                break;
                            case GameObj.typeObj.mine:
                                Map[x, y] = new Mine();
                                Mines.Add((Mine)Map[x, y]);
                                break;
                            case GameObj.typeObj.town:
                                Map[x, y] = new Town();
                                Towns.Add((Town)Map[x, y]);
                                break;
                        }
                        Map[x, y].type = (GameObj.typeObj)int.Parse(s[x]);
                        Map[x, y].Position = new Coord(x, y);
                    }
                }
                SpawnPos = new Coord[2, 3];
                for (int i = 0; i < 2; i++)
                {
                    s = fs.ReadLine().Split(';');
                    for (int j = 0; j < 3; j++)
                    {
                        string[] ss = s[j].Trim().Split(' ');
                        SpawnPos[i, j] = new Coord(int.Parse(ss[0]), int.Parse(ss[1]));
                    }
                }
            }
        }

        public void SpawnUnit(Unit u)
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
