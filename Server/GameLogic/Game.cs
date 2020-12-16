using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Server.GameLogic
{
    public class Game
    {
        public _Map map;
        private Mutex mutexMap;
        public Player[] players;
        public Player currentPlayer;

        public Game(_Map map, Player[] p) // expecting 2 players // OK
        {
            mutexMap = new Mutex();
            this.map = map;
            players = p;

            players[0].town = map.towns[0];
            players[0].NextTurn += players[0].town.nextTurn;
            players[0].town.owner = players[0];
            players[0].id = 0;

            players[1].town = map.towns[1];
            players[1].NextTurn += players[1].town.nextTurn;
            players[1].town.owner = players[1];
            players[1].id = 1;

            currentPlayer = players[0];
        }

        public void SelectUnit(Unit unit) // click (Left) // OK
        {
            if (unit.owner != currentPlayer) throw new Exception("This is enemy");
            currentPlayer.selectUnit = unit;
        }
        public Queue<int> MoveUnit(Coord A)  // click (Left) // OK
        {
            if (currentPlayer.selectUnit == null) throw new Exception("Not select unit");

            mutexMap.WaitOne();

            Queue<int> path = PathFinding(currentPlayer.selectUnit.Position, A);
            if (currentPlayer.selectUnit.actionPoints - path.Count < 0) throw new Exception("Not moving");
            var unit = currentPlayer.selectUnit;

            unit.actionPoints = 100; // delete
            currentPlayer.selectUnit.actionPoints -= path.Count;

            map.Map[unit.Position.X, unit.Position.Y] = new GameObj(GameObj.typeObj.empty);
            map.Map[unit.Position.X, unit.Position.Y].Position = unit.Position;
            map.Map[A.X, A.Y] = unit;
            unit.Position = A;

            mutexMap.ReleaseMutex();
            return path;
        }
        public GameObj[] Attack(Unit obj) // click (Right) // OK
        {
            if (currentPlayer.selectUnit == null) throw new Exception("Not select unit");
            if (obj.owner == currentPlayer) throw new Exception("your object");

            mutexMap.WaitOne();
            currentPlayer.selectUnit.atack(obj);
            if (obj.health <= 0)
            {
                Coord p = obj.Position;
                obj = (Unit)new GameObj();
                obj.type = GameObj.typeObj.empty;
                obj.Position = p;
                map.Map[p.X, p.Y] = obj;
            }
            mutexMap.ReleaseMutex();
            return new GameObj[] { currentPlayer.selectUnit, obj };
        }
        public GameObj[] Attack(Town obj)
        {
            if (currentPlayer.selectUnit == null) throw new Exception("Not select unit");
            if (obj.owner == currentPlayer) throw new Exception("your object");
            currentPlayer.selectUnit.atack(obj);
            return new GameObj[] { currentPlayer.selectUnit, obj };
        }

        public Unit SpawnUnit(Unit.typeUnit id, int level = 1) // interface buttons // OK
        {
            Unit u;
            switch (id)
            {
                case Unit.typeUnit.Scout:
                    u = new Scout(currentPlayer);
                    break;
                case Unit.typeUnit.Warior:
                    u = new Warior(currentPlayer, level);
                    break;
                case Unit.typeUnit.Shooter:
                    u = new Shooter(currentPlayer, level);
                    break;
                case Unit.typeUnit.Top:
                    u = new Top(currentPlayer);
                    break;
                default:
                    u = new Scout(currentPlayer);
                    break;
            }
            u.owner = currentPlayer;
            mutexMap.WaitOne();
            map.SpawnUnit(u);
            mutexMap.ReleaseMutex();
            currentPlayer.NextTurn += u.NextTurn;
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
            if (c > 1) throw new Exception("out of range");
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
            GameObj[,] Map = map.Map;
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
                        matrix[top, top - columns]     = Map[x, y - 1].type == 0 ? 1 : inf; // вверх
                    if (x != columns - 1 && y != 0)
                        matrix[top, top - columns + 1] = Map[x + 1, y - 1].type == 0 ? 2 : inf; // вправо вверх
                    if (x != columns - 1)
                        matrix[top, top + 1]           = Map[x + 1, y].type == 0 ? 1 : inf; // вправо
                    if (x != columns - 1 && y != lines - 1)
                        matrix[top, top + columns + 1] = Map[x + 1, y + 1].type == 0 ? 2 : inf; // вправо вниз
                    if (y != lines - 1)
                        matrix[top, top + columns]     = Map[x, y + 1].type == 0 ? 1 : inf; // вниз
                    if (x != 0 && y != lines - 1)
                        matrix[top, top + columns - 1] = Map[x - 1, y + 1].type == 0 ? 2 : inf; // влево вниз
                    if (x != 0)
                        matrix[top, top - 1]           = Map[x - 1, y].type == 0 ? 1 : inf; // влево
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
    }
}
