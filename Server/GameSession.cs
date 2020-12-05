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

        private void Player_onGameAction(GameActionPocket pocket, int id)
        {
            // Action proceed
            //Game.Action((Game.Buttons)pocket.Buttons, new Coord(pocket.CoordX, pocket.CoordY), pocket.Param);
        }
    }
}
