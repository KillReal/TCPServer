using System;
using System.Collections.Generic;
using System.Text;
using Server.GameLogic;
using Server.Pockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Server
{
    public class PlayerClient
    {
        public int idClient;
        public int idAponent;
        public Game game;
        public Player playerInGame;
        public bool cheatMode;
    }
    public class GameManager
    {
        public ClientManager clientManager;
        public List<Game> games;
        public Dictionary<int, PlayerClient> playerClients;
        //private const string map = @"..\..\..\Resources\Map.txt"; // path to the map file (for now one map)
        public enum Buttons
        {
            Left = 1,
            Right = 2,
            SpawnUnit = 3,
            UpgradeTown = 4,
            Market = 5,
            NextTurn = 6,
            Cheats = 7,
            GiveUp = 8,
        }
        
        public GameManager()
        {
            games = new List<Game>();
            playerClients = new Dictionary<int, PlayerClient>();
        }

        public void Init(ClientManager _clientManager)
        {
            clientManager = _clientManager;
            PocketHandler.onGameAction += HandleGameAction;
        }

        public void StartGame(List<int> idClients) // expecting 2 players // OK
        {
            if (idClients.Count != 2) throw new Exception("invalid number of clients");
            World world;
            using (FileStream fs = new FileStream(@"..\..\..\Resources\World", FileMode.OpenOrCreate))
                world = (World)new BinaryFormatter().Deserialize(fs);

            Game game = new Game(world, new Player[] { new Player("0"), new Player("1") });
            games.Add(game);
            playerClients.Add(idClients[0], new PlayerClient()
            {
                idClient = idClients[0],
                idAponent = idClients[1],
                game = game,
                playerInGame = game.players[0],
                cheatMode = false
            });
            playerClients.Add(idClients[1], new PlayerClient()
            {
                idClient = idClients[1],
                idAponent = idClients[0],
                game = game,
                playerInGame = game.players[1],
                cheatMode = false
            });
            clientManager.Send(idClients[0], new InitGamePocket(playerClients[idClients[0]].playerInGame));
            clientManager.Send(idClients[1], new InitGamePocket(playerClients[idClients[1]].playerInGame));
            clientManager.Send(idClients[0], new NextTurnPocket(playerClients[idClients[0]].game.currentPlayer));
            clientManager.Send(idClients[1], new NextTurnPocket(playerClients[idClients[1]].game.currentPlayer));
            clientManager.Send(idClients[0], new PlayerResourcesPocket(game.players[0]));
            clientManager.Send(idClients[1], new PlayerResourcesPocket(game.players[1]));
        }

        public void HandleGameAction(GameActionPocket pocket, int id)
        {

            PlayerClient client = playerClients[id];
            Game game = client.game;
            try
            {
                BasePocket data = null;
                if (client.cheatMode)
                {
                    client.playerInGame.gold = 1000;
                    client.playerInGame.wood = 1000;
                    client.playerInGame.rock = 1000;
                    client.playerInGame.crystall = 1000;
                    client.playerInGame.town.level = 1000;
                    if (client.playerInGame.selectUnit != null)
                        client.playerInGame.selectUnit.actionPoints = 1000;
                }

                switch (pocket.Button)
                {
                    case Buttons.GiveUp:
                        data = new EndGamePocket(playerClients[client.idAponent].playerInGame, 1);
                        break;
                    case Buttons.Cheats:
                        client.cheatMode = !client.cheatMode;
                        if (client.cheatMode) throw new Exception("CHEATS MOD ACTIVETED");
                        else throw new Exception("CHEATS MOD DE-ACTIVETED");
                        break;
                    case Buttons.SpawnUnit: // OK
                        data = new SpawnUnitPocket(game.SpawnUnit((Unit.typeUnit)pocket.Param));
                        clientManager.Send(client.idClient, new PlayerResourcesPocket(game.currentPlayer));
                        break;
                    case Buttons.UpgradeTown: // OK
                        game.UpgradeTown();
                        data = new UpgradeTownPocket(game.currentPlayer.town);
                        clientManager.Send(client.idClient, new PlayerResourcesPocket(game.currentPlayer));
                        break;
                    case Buttons.Market:
                        game.Market();
                        data = new MarketPocket(game.currentPlayer);
                        clientManager.Send(client.idClient, new PlayerResourcesPocket(game.currentPlayer));
                        break;
                    case Buttons.NextTurn: // OK
                        game.nextTurn();
                        data = new NextTurnPocket(game.currentPlayer);
                        clientManager.Send(client.idAponent, new PlayerResourcesPocket(game.currentPlayer));
                        if (game.currentPlayer.selectUnit != null)
                            clientManager.Send(client.idAponent, new SelectUnitPocket(game.currentPlayer.selectUnit));
                        break;
                    case Buttons.Left:
                    case Buttons.Right:
                        switch (game.world.Map[pocket.Coord.X, pocket.Coord.Y].type)
                        {
                            case GameObj.typeObj.empty when pocket.Button == Buttons.Left: // OK
                                Coord coord;
                                Queue<int> path;
                                (coord, path) = game.MoveUnit(new Coord(pocket.Coord.X, pocket.Coord.Y));
                                data = new MoveUnitPocket(game.currentPlayer.selectUnit, path, coord);
                                break;
                            case GameObj.typeObj.unit when pocket.Button == Buttons.Left:  // OK
                                game.SelectUnit((Unit)game.world.Map[pocket.Coord.X, pocket.Coord.Y]);
                                data = new SelectUnitPocket(game.currentPlayer.selectUnit);
                                break;
                            case GameObj.typeObj.unit when pocket.Button == Buttons.Right: // OK
                                {
                                    Unit unitA;
                                    Unit unitD;
                                    (unitA, unitD) = game.Attack((Unit)game.world.Map[pocket.Coord.X, pocket.Coord.Y]);
                                    data = new AttackPocket(unitA, unitD);
                                }
                                break;
                            case GameObj.typeObj.town when pocket.Button == Buttons.Right:
                                {
                                    Town town;
                                    Unit unit;
                                    (unit, town) = game.Attack((Town)game.world.Map[pocket.Coord.X, pocket.Coord.Y]);
                                    if (town.health == 0)
                                    {
                                        data = new EndGamePocket(game.currentPlayer, 1);
                                        // END GAME... (below code, send packet on end game!!!)   
                                    }
                                    else
                                    {
                                        data = new AttackTownPocket(unit, town);
                                    }
                                }
                                break;
                            case GameObj.typeObj.mine when pocket.Button == Buttons.Right:  // OK
                                data = new CaptureMinePocket(game.CaptureMine((Mine)game.world.Map[pocket.Coord.X, pocket.Coord.Y]));
                                break;
                            default:
                                throw new Exception("block!"); // OK
                        }
                        break;
                    default:
                        throw new Exception("undefined");
                }
                clientManager.Send(client.idClient, data);  // 1
                clientManager.Send(client.idAponent, data); // 2
            }
            catch (Exception e)
            {
                clientManager.Send(id, new ErrorPocket(228, e.Message)); // add type Exception
            }
        }

        public void endGame(List<int> idClients)
        {

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