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
        public enum Buttons
        {
            Left,
            Right,
            SpawnUnit,
            UpgradeTown,
            Market,
            NextTurn
        }


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
            // SANIN ALG // 
            int costMoving = 0;
            if (currentPlayer.selectUnit.actionPoints - costMoving < 0) throw new Exception("Not moving");
            return null;
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
    }
}
