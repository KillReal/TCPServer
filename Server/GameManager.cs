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
            Left,
            Right,
            SpawnUnit,
            UpgradeTown,
            Market,
            NextTurn
        }

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
            Game game = playerClients[id].game;
            try
            {
                switch ((Buttons)pocket.Button)
                {
                    case Buttons.SpawnUnit:
                        clientManager.Send(
                            id, 
                            new SpawnUnitPocket(
                                game.SpawnUnit(
                                    (Unit.typeUnit)pocket.Param
                                )
                            ).ToBytes()
                        );
                        break;
                    case Buttons.UpgradeTown:
                        game.UpgradeTown();
                        clientManager.Send(id, new UpgradeTownPocket(game.currentPlayer.town).ToBytes());
                        break;
                    case Buttons.Market:
                        game.Market();
                        clientManager.Send(id, new MarketPocket(game.currentPlayer).ToBytes());
                        break;
                    case Buttons.NextTurn:
                        game.nextTurn();
                        clientManager.Send(id, new NextTurnPocket(game.currentPlayer).ToBytes());
                        break;
                    case Buttons.Left:
                    case Buttons.Right:
                        switch (game.map.Map[pocket.CoordX, pocket.CoordY].type)
                        {
                            case GameObj.typeObj.empty when (Buttons)pocket.Button == Buttons.Left:
                                clientManager.Send(id, new MoveUnitPocket(game.currentPlayer.selectUnit, game.MoveUnit(new Coord(pocket.CoordX, pocket.CoordY))).ToBytes());
                                break;
                            case GameObj.typeObj.unit when (Buttons)pocket.Button == Buttons.Left:
                                game.SelectUnit((Unit)game.map.Map[pocket.CoordX, pocket.CoordY]);
                                clientManager.Send(id, new SelectUnitPocket(game.currentPlayer.selectUnit).ToBytes());
                                break;
                            case GameObj.typeObj.unit when (Buttons)pocket.Button == Buttons.Right:
                            case GameObj.typeObj.town when (Buttons)pocket.Button == Buttons.Right:
                                GameObj[] gm = game.Attack(game.map.Map[pocket.CoordX, pocket.CoordY]);
                                clientManager.Send(id, new AttackPocket(gm[0], gm[1]).ToBytes());
                                break;
                            case GameObj.typeObj.mine when (Buttons)pocket.Button == Buttons.Right:
                                clientManager.Send(id, new CaptureMinePocket(game.CaptureMine((Mine)game.map.Map[pocket.CoordX, pocket.CoordY])).ToBytes());
                                break;
                            // Market:...
                        }
                        break;
                }
                //return null;
            }
            catch(Exception e)
            {
                //return e;
            }
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
