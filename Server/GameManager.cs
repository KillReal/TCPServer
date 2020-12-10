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
            Game game = playerClients[id].game;
            byte[] data = null;
            try
            {
                switch (pocket.Button)
                {
                    case Buttons.SpawnUnit:
                        data = new SpawnUnitPocket(game.SpawnUnit((Unit.typeUnit)pocket.Param)).ToBytes();
                        break;
                    case Buttons.UpgradeTown:
                        game.UpgradeTown();
                        data = new UpgradeTownPocket(game.currentPlayer.town).ToBytes();
                        break;
                    case Buttons.Market:
                        game.Market();
                        data = new MarketPocket(game.currentPlayer).ToBytes();
                        break;
                    case Buttons.NextTurn:
                        game.nextTurn();
                        data = new NextTurnPocket(game.currentPlayer).ToBytes();
                        break;
                    case Buttons.Left:
                    case Buttons.Right:
                        switch (game.map.Map[pocket.Coord.X, pocket.Coord.Y].type)
                        {
                            case GameObj.typeObj.empty when pocket.Button == Buttons.Left:
                                data = new MoveUnitPocket(game.currentPlayer.selectUnit, game.MoveUnit(new Coord(pocket.Coord.X, pocket.Coord.Y))).ToBytes();
                                break;
                            case GameObj.typeObj.unit when pocket.Button == Buttons.Left:
                                game.SelectUnit((Unit)game.map.Map[pocket.Coord.X, pocket.Coord.Y]);
                                data = new SelectUnitPocket(game.currentPlayer.selectUnit).ToBytes();
                                break;
                            case GameObj.typeObj.unit when pocket.Button == Buttons.Right:
                            case GameObj.typeObj.town when pocket.Button == Buttons.Right:
                                GameObj[] gm = game.Attack(game.map.Map[pocket.Coord.X, pocket.Coord.Y]);
                                data = new AttackPocket(gm[0], gm[1]).ToBytes();
                                break;
                            case GameObj.typeObj.mine when pocket.Button == Buttons.Right:
                                data = new CaptureMinePocket(game.CaptureMine((Mine)game.map.Map[pocket.Coord.X, pocket.Coord.Y])).ToBytes();
                                break;
                            default:
                                throw new Exception("block!");
                            // Market:...
                        }
                        break;
                    default:
                        throw new Exception("undefined");
                }
                clientManager.Send(id, data); // 1
                //clientManager.Send(id, data); // 2
            }
            catch(Exception e)
            {
                clientManager.Send(id, new ErrorPocket(228, e.Message).ToBytes()); // add type Exception
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
