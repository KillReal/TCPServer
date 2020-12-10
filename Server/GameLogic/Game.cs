using System;
using System.Collections.Generic;
using System.Text;

namespace Server.GameLogic
{
    public class Game
    {
        public _Map map;
        public Player[] players;
        public Player currentPlayer;

        public Game(_Map map, Player[] p) // expecting 2 players
        {
            this.map = map;
            players = p;
            players[0].town = map.Towns[0];
            players[0].id = 0;
            players[1].town = map.Towns[1];
            players[1].id = 1;
            currentPlayer = players[0];
        }

        public void SelectUnit(Unit unit) // click (Left)
        {
            if (unit.owner != currentPlayer) throw new Exception("This is enemy");
            currentPlayer.selectUnit = unit;
        }
        public int[] MoveUnit(Coord A)  // click (Left)
        {
            if (currentPlayer.selectUnit == null) throw new Exception("Not select unit");
            int[] path = PathFinding(currentPlayer.selectUnit.Position, A);
            if (currentPlayer.selectUnit.actionPoints - path.Length < 0) throw new Exception("Not moving");
            var unit = currentPlayer.selectUnit;
            map.Map[unit.Position.X, unit.Position.Y] = new GameObj();
            map.Map[unit.Position.X, unit.Position.Y].type = GameObj.typeObj.empty;
            map.Map[unit.Position.X, unit.Position.Y].Position = unit.Position;
            map.Map[A.X, A.Y] = unit;
            unit.Position = A;
            return path;
        }
        public GameObj[] Attack(GameObj obj) // click (Right)
        {
            if (currentPlayer.selectUnit == null) throw new Exception("Not select unit");
            currentPlayer.selectUnit.atack(obj);
            // CHECK DIE! and maybe delete object in map//
            return new GameObj[] { currentPlayer.selectUnit, obj };
        }

        public Unit SpawnUnit(Unit.typeUnit id, int level = 1) // interface buttons
        {
            Unit u = null;
            switch (id)
            {
                case Unit.typeUnit.Scout:
                    u = new Scout();
                    break;
                case Unit.typeUnit.Warior:
                    u = new Warior(level);
                    break;
                case Unit.typeUnit.Shooter:
                    u = new Shooter(level);
                    break;
                case Unit.typeUnit.Top:
                    u = new Top();
                    break;
            }
            u.owner = currentPlayer;
            map.SpawnUnit(u);
            return u;
        }
        public void UpgradeTown() // interface buttons
        {
            currentPlayer.town.upgrade();
        }
        public void Market() // Params: type trading
        {

        }
        public Mine CaptureMine(Mine mine) // click (Right)
        {
            Coord c = (currentPlayer.selectUnit.Position - mine.Position).ABS;
            if (c.X > 1 || c.Y > 1) throw new Exception("out of range");
            mine.owner = currentPlayer;
            return mine;
        }

        public void nextTurn()
        {
            currentPlayer = currentPlayer == players[0]
                ? players[1]
                : players[0];
        }

        // ... //

        public int[] PathFinding(Coord posA, Coord posB)
        {
            GameObj[,] Map = map.Map;
            const int inf = 1000000;
            const int lines = 51;
            const int columns = 71;
            int[,] matrix = new int[lines * columns, lines * columns];
            int[,] path = new int[lines * columns, lines * columns];
            int[] flag = new int[lines * columns];
            int[] dist = new int[lines * columns];

            for (int i = 0; i < lines * columns; i++)
                for (int j = 0; j < lines * columns; j++)
                {
                    matrix[i, j] = inf;
                    path[i, j] = inf;
                }


            for (int y = 0; y < lines; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    if (posA.X != x && posA.Y != y)
                        if (Map[x, y].type != 0) continue;
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
            }

            // номер 1 вершины
            int topS = posA.Y * columns + posA.X;
            // номер 2 вершины
            int topE = posB.Y * columns + posB.X;

            for (int i = 0; i < lines * columns; i++)
                dist[i] = inf;

            // Дейкстра
            dist[topS] = 0;
            int pos = 0;
            for (int i = 0; i < lines * columns - 1; i++)
            {
                int num = inf;
                for (int j = 0; j < lines * columns; j++)
                    if (dist[j] < num && flag[j] == 0)
                    {
                        num = dist[j];
                        pos = j;
                    }

                flag[pos] = 1;

                for (int j = 0; j < lines * columns; j++)
                    if (dist[j] > dist[pos] + matrix[pos, j])
                        dist[j] = dist[pos] + matrix[pos, j];
            }

            int[] ver = new int[lines * columns]; // массив посещенных вершин
            for (int i = 0; i < lines * columns; i++)
            {
                ver[i] = inf;
            }

            int count = 0;

            int end = topE; // индекс конечной вершины 
            ver[0] = end; // начальный элемент - конечная вершина
            int k = 1; // индекс предыдущей вершины
            int weight = dist[end]; // вес конечной вершины

            while (end != topS) // пока не дошли до начальной вершины
            {
                for (int i = 0; i < lines * columns; i++)
                {// просматриваем все вершины
                    if (matrix[i, end] != inf)   // если связь есть
                    {
                        int temp = weight - matrix[i, end]; // определяем вес пути из предыдущей вершины
                        if (temp == dist[i]) // если вес совпал с рассчитанным
                        {                 // значит из этой вершины и был переход
                            weight = temp; // сохраняем новый вес
                            end = i;       // сохраняем предыдущую вершину
                            ver[k] = i; // и записываем ее в массив
                            k++;
                            count++;
                        }
                    }
                }
            }

            for (int i = 0; i < lines * columns / 2; i++)
            {
                //(ver[i], ver[lines * columns - i - 1]) = (ver[lines * columns - i - 1], ver[i]);
                k = ver[i];
                ver[i] = ver[lines * columns - i - 1];
                ver[lines * columns - i - 1] = k;
            }

            int[] anss = new int[count + 1];
            int[] ans = new int[count];

            k = 0;

            for (int i = 0; i < lines * columns; i++)
            {
                if (ver[i] != inf)
                {
                    anss[k] = ver[i];
                    k++;
                }
            }

            for (int i = 0; i < count; i++)
            {
                int ii = anss[i];
                int jj = anss[i + 1];
                Coord p1 = new Coord(ii % columns, ii / columns);
                Coord p2 = new Coord(jj % columns, jj / columns);
                Coord p = p2 - p1;
                if (p == new Coord(-1, -1)) // left up
                    ans[i] = 1;
                else if (p == new Coord(0, -1)) // up
                    ans[i] = 2;
                else if (p == new Coord(1, -1)) // right up
                    ans[i] = 3;
                else if (p == new Coord(1, 0)) // right
                    ans[i] = 4;
                else if (p == new Coord(1, 1)) // right down
                    ans[i] = 5;
                else if (p == new Coord(0, 1)) // down
                    ans[i] = 6;
                else if (p == new Coord(-1, 1)) // left down
                    ans[i] = 7;
                else if (p == new Coord(-1, 0)) // left
                    ans[i] = 8;
            }

            return ans;
        }
    }
}
