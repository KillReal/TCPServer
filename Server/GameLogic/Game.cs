﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Server.GameLogic
{
    [Serializable]
    public class Game
    {
        public World world;
        private Mutex mutexMap;
        public Player[] players;
        public Player currentPlayer;

        public Game(World world, Player[] p) // expecting 2 players // OK
        {
            mutexMap = new Mutex();
            this.world = world;
            players = p;

            players[0].town = world.towns[0];
            players[0].NextTurn += players[0].town.nextTurn;
            players[0].town.owner = players[0];
            players[0].town.level = 0;
            players[0].town.health = 20;
            players[0].id = 0;

            players[1].town = world.towns[1];
            players[1].NextTurn += players[1].town.nextTurn;
            players[1].town.owner = players[1];
            players[1].town.level = 0;
            players[1].town.health = 20;
            players[1].id = 1;

            currentPlayer = players[1]; 
            nextTurn(); // problem first turn... (player gold = 100!!)
        }

        public void SelectUnit(Unit unit) // click (Left) // OK
        {
            //mutexMap.WaitOne();
            if (unit.owner != currentPlayer) throw new Exception("This is enemy");
            currentPlayer.selectUnit = unit;
            //mutexMap.ReleaseMutex();
        }
        public (Coord, Queue<int>) MoveUnit(Coord A)  // click (Left) // OK
        {
            //mutexMap.WaitOne();
            if (currentPlayer.selectUnit == null) throw new Exception("Not select unit");

            var unit = currentPlayer.selectUnit;
            Coord start = unit.Position;
            Queue<int> path = PathFinding(start, A);
            // for debug inf actionPoints, on release remove comment
            if (unit.actionPoints - path.Count < 0) throw new Exception("Not moving");

            unit.actionPoints -= path.Count;

            world.Map[unit.Position.X, unit.Position.Y] = new GameObj(GameObj.typeObj.empty);
            world.Map[unit.Position.X, unit.Position.Y].Position = unit.Position;
            world.Map[A.X, A.Y] = unit;
            unit.Position = A;

            //mutexMap.ReleaseMutex();
            return (start, path);
        }
        public (Unit, Unit) Attack(Unit unit) // click (Right) // OK
        {
            //mutexMap.WaitOne();
            if (currentPlayer.selectUnit == null) throw new Exception("Not select unit");
            if (unit.owner == currentPlayer) throw new Exception("your object");

            currentPlayer.selectUnit.atack(unit);
            if (unit.health <= 0)
            {
                if (unit.owner.selectUnit == unit)
                    unit.owner.selectUnit = null;
                Coord p = unit.Position;
                var o = new GameObj();
                o.type = GameObj.typeObj.empty;
                o.Position = p;
                world.Map[p.X, p.Y] = o;

            }
            //mutexMap.ReleaseMutex();
            return (currentPlayer.selectUnit, unit);
        }
        public (Unit, Town) Attack(Town town)
        {
            if (currentPlayer.selectUnit == null) throw new Exception("Not select unit");
            if (town.owner == currentPlayer) throw new Exception("your object");

            //currentPlayer.selectUnit.atack(town);
            town.health--;
            return (currentPlayer.selectUnit, town);
        }

        public Unit SpawnUnit(Unit.typeUnit id) // interface buttons // OK
        {
            //mutexMap.WaitOne();
            Coord pos = world.SpawnUnit(currentPlayer);
            Unit u;
            switch (id)
            {
                case Unit.typeUnit.Scout:
                    u = new Scout(currentPlayer);
                    break;
                case Unit.typeUnit.Warrior1:
                    u = new Warior(currentPlayer, (int)id);
                    break;
                case Unit.typeUnit.Shooter1:
                    u = new Shooter(currentPlayer, (int)id);
                    break;
                case Unit.typeUnit.Warrior2:
                    u = new Warior(currentPlayer, (int)id);
                    break;
                case Unit.typeUnit.Shooter2:
                    u = new Shooter(currentPlayer, (int)id);
                    break;
                case Unit.typeUnit.Warrior3:
                    u = new Warior(currentPlayer, (int)id);
                    break;
                case Unit.typeUnit.Shooter3:
                    u = new Shooter(currentPlayer, (int)id);
                    break;
                case Unit.typeUnit.Top:
                    u = new Top(currentPlayer);
                    break;
                default:
                    u = new Scout(currentPlayer);
                    break;
            }
            u.Position = pos;
            u.type = GameObj.typeObj.unit;
            u.owner = currentPlayer;
            world.Map[pos.X, pos.Y] = u;
            currentPlayer.NextTurn += u.NextTurn;
            //mutexMap.ReleaseMutex();
            return u;
        }
        public void UpgradeTown() // interface buttons // OK
        {
            currentPlayer.town.upgrade();
        }
        public void Market() // Params: type trading
        {

        }
        public Mine CaptureMine(Mine mine) // click (Right) // OK
        {
            if (currentPlayer.selectUnit == null) throw new Exception("Not select unit");
            Coord c = (currentPlayer.selectUnit.Position - mine.Position).ABS;
            if (c > 1.5) throw new Exception("out of range");
            if (mine.owner != null)
                mine.owner.NextTurn -= mine.mining; 
            mine.owner = currentPlayer;
            currentPlayer.NextTurn += mine.mining;
            return mine;
        }

        public void nextTurn() // OK
        {
            currentPlayer = currentPlayer == players[0]
                ? players[1]
                : players[0];
            currentPlayer.Turn();
        }

        // ... //

        private Queue<int> PathFinding(Coord posA, Coord posB) // OK (can be optimal)
        {
            GameObj[,] Map = world.Map;
            const int inf = 1000000;
            int lines = Map.GetUpperBound(1) + 1;
            int columns = Map.GetUpperBound(0) + 1;
            int[,] matrix = new int[lines * columns, lines * columns];

            for (int i = 0; i < lines * columns; i++)
                for (int j = 0; j < lines * columns; j++)
                    matrix[i, j] = inf;

            for (int y = 0; y < lines; y++)
                for (int x = 0; x < columns; x++)
                {
                    int top = y * columns + x; // ME!
                    if (x != 0 && y != 0)
                        matrix[top, top - columns - 1] = Map[x - 1, y - 1].type == 0 ? 2 : inf; // лево вверх
                    if (y != 0)
                        matrix[top, top - columns] = Map[x, y - 1].type == 0 ? 1 : inf; // вверх
                    if (x != columns - 1 && y != 0)
                        matrix[top, top - columns + 1] = Map[x + 1, y - 1].type == 0 ? 2 : inf; // вправо вверх
                    if (x != columns - 1)
                        matrix[top, top + 1] = Map[x + 1, y].type == 0 ? 1 : inf; // вправо
                    if (x != columns - 1 && y != lines - 1)
                        matrix[top, top + columns + 1] = Map[x + 1, y + 1].type == 0 ? 2 : inf; // вправо вниз
                    if (y != lines - 1)
                        matrix[top, top + columns] = Map[x, y + 1].type == 0 ? 1 : inf; // вниз
                    if (x != 0 && y != lines - 1)
                        matrix[top, top + columns - 1] = Map[x - 1, y + 1].type == 0 ? 2 : inf; // влево вниз
                    if (x != 0)
                        matrix[top, top - 1] = Map[x - 1, y].type == 0 ? 1 : inf; // влево
                }

            int topS = posA.Y * columns + posA.X;
            int topE = posB.Y * columns + posB.X;

            bool[] flag = new bool[lines * columns];
            int[] dist = new int[lines * columns];
            Array.Fill<int>(dist, inf);

            // Дейкстра
            dist[topS] = 0;
            int pos = 0;
            for (int i = 0; i < lines * columns - 1; i++)
            {
                int num = inf;
                for (int j = 0; j < lines * columns; j++)
                    if (dist[j] < num && !flag[j])
                    {
                        num = dist[j];
                        pos = j;
                    }

                flag[pos] = true;

                for (int j = 0; j < lines * columns; j++)
                    if (dist[j] > dist[pos] + matrix[pos, j])
                        dist[j] = dist[pos] + matrix[pos, j];
            }

            if (dist[topE] == inf) throw new Exception("no way");

            Stack<int> visited = new Stack<int>(); // массив посещенных вершин
            int end = topE; // индекс конечной вершины // можно тоже упростить с помощью стека
            visited.Push(end); // начальный элемент - конечная вершина
            int weight = dist[end]; // вес конечной вершины

            while (end != topS) // пока не дошли до начальной вершины
                for (int i = 0; i < lines * columns; i++) // просматриваем все вершины
                    if (matrix[i, end] != inf)   // если связь есть
                    {
                        int temp = weight - matrix[i, end]; // определяем вес пути из предыдущей вершины
                        if (temp == dist[i]) // если вес совпал с рассчитанным
                        {                 // значит из этой вершины и был переход
                            weight = temp; // сохраняем новый вес
                            end = i;       // сохраняем предыдущую вершину
                            visited.Push(i); // и записываем ее в массив
                        }
                    }

            Queue<int> ans = new Queue<int>();
            for (int i = visited.Pop(); visited.Count != 0; i = visited.Pop())
            {
                int j = visited.Peek();
                Coord p1 = new Coord(i % columns, i / columns);
                Coord p2 = new Coord(j % columns, j / columns);
                Coord p = p2 - p1;
                if (p == new Coord(-1, -1))     // left up
                    ans.Enqueue(1);
                else if (p == new Coord(0, -1)) // up
                    ans.Enqueue(2);
                else if (p == new Coord(1, -1)) // right up
                    ans.Enqueue(3);
                else if (p == new Coord(1, 0)) // right
                    ans.Enqueue(4);
                else if (p == new Coord(1, 1)) // right down
                    ans.Enqueue(5);
                else if (p == new Coord(0, 1)) // down
                    ans.Enqueue(6);
                else if (p == new Coord(-1, 1)) // left down
                    ans.Enqueue(7);
                else if (p == new Coord(-1, 0)) // left
                    ans.Enqueue(8);
            }

            return ans;
        }

        private void SaveGame(int id)
        {
            using (FileStream fs = new FileStream($"SaveGame-{id}", FileMode.OpenOrCreate))
                new BinaryFormatter().Serialize(fs, this);
        }
    }
}