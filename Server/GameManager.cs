using System;
using System.Collections.Generic;
using System.Text;
using Server.GameLogic;
using Server.Pockets;
using Server.Enums;

namespace Server
{  
    public struct PlayerClient
    {
        public int idClient;
        public Game game;
        public Player playerInGame;
    }
    public class GameManager
    {
        public ClientManager clientManager;
        public List<Game> games;
        public Dictionary<int, PlayerClient> playerClients;
        private const string map = ""; // path to the map file (for now one map)

        public GameManager()
        {
            PocketHandler.onGameAction += Player_onGameAction;
        }

        public void Init(ClientManager _clientManager)
        {
            clientManager = _clientManager;
        }

        public void StartGame(List<int> idClients) // expecting 2 players
        {
            if (idClients.Count != 2) throw new Exception("invalid number of clients");

            Game game = new Game(new _Map(map), new Player[] { new Player("0"), new Player("1") });
            games.Add(game);
            playerClients.Add(idClients[0], new PlayerClient() {
                idClient = idClients[0],
                game = game,
                playerInGame = game.players[0],
            });
            playerClients.Add(idClients[1], new PlayerClient() {
                idClient = idClients[1],
                game = game,
                playerInGame = game.players[1],
            });
        }

        private void Player_onGameAction(GameActionPocket pocket, int id)
        {
            playerClients[id].game.Action((Game.Buttons)pocket.Buttons, new Coord(pocket.CoordX, pocket.CoordY), pocket.Param);
        }

        public void checkClient(int id)
        {
            // is there a client in the game?
            if (clientManager.GetClientState(id) == ClientStateEnum.Connected)
            {
                // ??
            }
        }
    }
}
