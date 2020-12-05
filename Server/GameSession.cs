using Server.Pockets;
using Server.GameLogic;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    class GameSession
    {
        public GameSession()
        {
            PocketHandler.onGameAction += Player_onGameAction;
        }

        public void CreatePlayer(int id)
        {

        }

        private void Player_onGameAction(GameActionPocket pocket, int id)
        {
            
            Game.Action(pocket)
        }
    }
}
