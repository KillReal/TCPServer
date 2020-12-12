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
        public int idAponent;
        public Game game;
        public Player playerInGame;
    }
    public class GameManager
    {
        public ClientManager clientManager;
        public List<Game> games;
        public Dictionary<int, PlayerClient> playerClients;
        private const string map = @"..\..\..\Resources\Map.txt"; // "~/Resources/Map.txt"; // path to the map file (for now one map)
        public enum Buttons
        {
            Left = 1,
            Right = 2,
            SpawnUnit = 3,
            UpgradeTown = 4,
            Market = 5,
            NextTurn = 6
        }

        public GameManager()
        {
            PocketHandler.onGameAction += Player_onGameAction;
            games = new List<Game>();
            playerClients = new Dictionary<int, PlayerClient>();
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
                idAponent = idClients[1],
                game = game,
                playerInGame = game.players[0],
            });
            playerClients.Add(idClients[1], new PlayerClient() {
                idClient = idClients[1],
                idAponent = idClients[0],
                game = game,
                playerInGame = game.players[1],
            });
            clientManager.Send(idClients[0], new InitGamePocket(playerClients[idClients[0]].playerInGame));
            clientManager.Send(idClients[1], new InitGamePocket(playerClients[idClients[1]].playerInGame));
            clientManager.Send(idClients[0], new NextTurnPocket(playerClients[idClients[0]].game.currentPlayer));
            clientManager.Send(idClients[1], new NextTurnPocket(playerClients[idClients[1]].game.currentPlayer));
        }

        private void Player_onGameAction(GameActionPocket pocket, int id)
        {

            Game game = playerClients[id].game;
            try
            {
                BasePocket data;
                switch (pocket.Button)
                {
                    case Buttons.SpawnUnit: // OK
                        data = new SpawnUnitPocket(game.SpawnUnit((Unit.typeUnit)pocket.Param));
                        break;
                    case Buttons.UpgradeTown: // OK
                        game.UpgradeTown();
                        data = new UpgradeTownPocket(game.currentPlayer.town);
                        break;
                    case Buttons.Market:
                        game.Market();
                        data = new MarketPocket(game.currentPlayer);
                        break;
                    case Buttons.NextTurn: // OK
                        game.nextTurn();
                        data = new NextTurnPocket(game.currentPlayer);
                        break;
                    case Buttons.Left:
                    case Buttons.Right:
                        switch (game.map.Map[pocket.Coord.X, pocket.Coord.Y].type)
                        {
                            case GameObj.typeObj.empty when pocket.Button == Buttons.Left: // OK
                                data = new MoveUnitPocket(game.currentPlayer.selectUnit, game.MoveUnit(new Coord(pocket.Coord.X, pocket.Coord.Y)));
                                break;
                            case GameObj.typeObj.unit when pocket.Button == Buttons.Left:  // OK
                                game.SelectUnit((Unit)game.map.Map[pocket.Coord.X, pocket.Coord.Y]);
                                data = new SelectUnitPocket(game.currentPlayer.selectUnit);
                                break;
                            case GameObj.typeObj.unit when pocket.Button == Buttons.Right:
                            case GameObj.typeObj.town when pocket.Button == Buttons.Right:
                                GameObj[] gm = game.Attack(game.map.Map[pocket.Coord.X, pocket.Coord.Y]);
                                data = new AttackPocket(gm[0], gm[1]);
                                break;
                            case GameObj.typeObj.mine when pocket.Button == Buttons.Right:  // OK
                                data = new CaptureMinePocket(game.CaptureMine((Mine)game.map.Map[pocket.Coord.X, pocket.Coord.Y]));
                                break;
                            default:
                                throw new Exception("block!");
                                // Market:...
                        }
                        break;
                    default:
                        throw new Exception("undefined");
                }
                clientManager.Send(playerClients[id].idClient, data);  // 1
                clientManager.Send(playerClients[id].idAponent, data); // 2
            }
            catch (Exception e)
            {
                clientManager.Send(id, new ErrorPocket(228, e.Message)); // add type Exception
            }
        }

        public void checkClient(int id)
        {
            // is there a client in the game?
            if (clientManager.GetClientState(id) >= ClientStateEnum.Connected)
            {
                // ??
            }
        }
    }
}
