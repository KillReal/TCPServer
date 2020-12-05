using System;
using System.Collections.Generic;
using System.Text;
using Server.GameLogic;

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
        public List<Game> games;
        public Dictionary<int, PlayerClient> playerClients;
        private const string map = ""; // path to the map file (for now one map)
        public void StartGame(int[] idClients) // expecting 2 players
        {
            if (idClients.Length != 2) throw new Exception("invalid number of clients");

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
        public void Action(int idClient, Game.Buttons button, Coord coord, int prm)
        {
            playerClients[idClient].game.Action(button, coord, prm);
        }
        public void checkClient()
        {
            // is there a client in the game?
        }
    }
}
