using System;
using System.Collections.Generic;
using System.Text;

namespace Server.GameLogic
{
    public class Player
    {
        public string name;
        public int id;
        public int gold, rock, wood, crystall;
        public Unit selectUnit;
        public Town town;
        public event Action NextTurn;
        public void Turn()
        {
            NextTurn?.Invoke();
        }
        public Player(string name)
        {
            this.name = name;
            this.gold = 0;
            this.wood = 0;
            this.rock = 0;
            this.crystall = 0;
        }
    }
}
