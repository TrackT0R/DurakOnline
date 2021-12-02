using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace DurakWcf
{
    // ПРИМЕЧАНИЕ. Команду "Переименовать" в меню "Рефакторинг" можно использовать для одновременного изменения имени класса "Service1" в коде, SVC-файле и файле конфигурации.
    // ПРИМЕЧАНИЕ. Чтобы запустить клиент проверки WCF для тестирования службы, выберите элементы Service1.svc или Service1.svc.cs в обозревателе решений и начните отладку.
    public class DurakService : IDurakService
    {
        #region Room lists
        private static List<Room> FreeRooms = new List<Room>();
        private static List<Room> GameRooms = new List<Room>();
        #endregion

        #region Connect methods
        public bool HasPassword(string RoomName)
        {
            int index = FreeRooms.FindIndex(x => x.RoomName == RoomName);
            return FreeRooms[index].password.Length > 0;
        }

        public bool ConnectRoom(string RoomName, string password, int PlayerId)
        {
            int index = FreeRooms.FindIndex(x => x.RoomName == RoomName);
            if (index == -1)
                return false;

            if (FreeRooms[index].password == password) {
                if (!FreeRooms[index].AddSecondPlayerID(PlayerId))
                    return false;
                GameRooms.Add(FreeRooms[index]);
                FreeRooms.RemoveAt(index);
                return true;
            }
            return false;
        }

        public bool CreateRoom(string RoomName, string password, int HostId)
        {
            if (FreeRooms.FindIndex(x => x.RoomName == RoomName) >= 0 || GameRooms.FindIndex(x => x.RoomName == RoomName) >= 0)
                return false;

            FreeRooms.Add(new Room(RoomName, password, HostId));
            return true;
        }

        public bool DeleteRoom(string RoomName, string password, int PlayerID)
        {
            int index = FreeRooms.FindIndex(x => x.RoomName == RoomName);
            if (index != -1)
                if (FreeRooms[index].password == password && ((FreeRooms[index].FirstPlayerID == PlayerID) || (FreeRooms[index].SecondPlayerID == PlayerID))) {
                    FreeRooms.RemoveAt(index);
                    return true;
                }

            index = GameRooms.FindIndex(x => x.RoomName == RoomName);
            if (index != -1)
                if (GameRooms[index].password == password && ((GameRooms[index].FirstPlayerID == PlayerID) || (GameRooms[index].SecondPlayerID == PlayerID))) {
                    GameRooms.RemoveAt(index);
                    return true;
                }

            return false;
        }

        public string[] GetFreeRooms()
        {
            var t = new string[FreeRooms.Count];
            for (int i = 0; i < FreeRooms.Count; i++)
                t[i] = FreeRooms[i].RoomName;
            return t;
        }
        #endregion

        #region Game methods

        private int GetGameRoomAndAuth(string RoomName, string password, int PlayerID)
        {
            int index = GameRooms.FindIndex(x => x.RoomName == RoomName);
            if (index == -1 || GameRooms[index].password != password ||
                (PlayerID != GameRooms[index].FirstPlayerID && PlayerID != GameRooms[index].SecondPlayerID))
                return -1;
            return index;
        }

        public Card Card(Card.SuitEnum suit, Card.ValueEnum value)
        {
            return new Card(suit, value);
        }

        #region Cards info requests
        public List<Card> GetMyCards(string RoomName, string password, int PlayerID)
        {
            int index = GameRooms.FindIndex(x => x.RoomName == RoomName);
            if (index == -1 || GameRooms[index].password != password || (PlayerID != GameRooms[index].FirstPlayerID && PlayerID != GameRooms[index].SecondPlayerID))
                return null;

            return GameRooms[index].FirstPlayerID == PlayerID ? GameRooms[index].FirstPlayerCards : GameRooms[index].SecondPlayerCards;
        }

        public List<List<Card>> GetCardsOnTable(string RoomName, string password, int PlayerID)
        {
            int index = GetGameRoomAndAuth(RoomName, password, PlayerID);
            return index == -1 ? null : GameRooms[index].CardsOnTable;
        }

        public int GetCardsInStockCount(string RoomName, string password, int PlayerID)
        {
            int index = GetGameRoomAndAuth(RoomName, password, PlayerID);
            return index == -1 ? -1 : GameRooms[index].CardsInStock.Count;
        }

        public int GetOpponentCardsCount(string RoomName, string password, int PlayerID)
        {
            int index = GetGameRoomAndAuth(RoomName, password, PlayerID);
            return index == -1 ? -1 : GameRooms[index].FirstPlayerID == PlayerID ?
                GameRooms[index].SecondPlayerCards.Count : GameRooms[index].FirstPlayerCards.Count;
        }

        public Card GetTrumpCard(string RoomName, string password, int PlayerID)
        {
            int index = GetGameRoomAndAuth(RoomName, password, PlayerID);
            return index == -1 ? null : GameRooms[index].TrumpCard;
        }
        #endregion

        #region Move processing
        public MoveOpportunity GetMoveOpportunity(string RoomName, string password, int PlayerID)
        {
            int index = GetGameRoomAndAuth(RoomName, password, PlayerID);
            if (index == -1) return null;

            if (GameRooms[index].FirstPlayerID == PlayerID)
                switch (GameRooms[index].GameStatus) {
                    case 1: return new MoveOpportunity(MoveOpportunity.CanMakeMoveEnum.CanAttack);
                    case 2: return new MoveOpportunity(MoveOpportunity.CanMakeMoveEnum.CanNothing);
                    case 11: return new MoveOpportunity(MoveOpportunity.CanMakeMoveEnum.CanDefend);
                    case 12: return new MoveOpportunity(MoveOpportunity.CanMakeMoveEnum.CanNothing);
                    case 21: return new MoveOpportunity(MoveOpportunity.CanMakeMoveEnum.CanThrow);
                    case 22: return new MoveOpportunity(MoveOpportunity.CanMakeMoveEnum.CanThrowAfter);
                    case 10: return new MoveOpportunity(MoveOpportunity.CanMakeMoveEnum.YouWin);
                    case 20: return new MoveOpportunity(MoveOpportunity.CanMakeMoveEnum.YouLose);
                }
            else
                switch (GameRooms[index].GameStatus) {
                    case 1: return new MoveOpportunity(MoveOpportunity.CanMakeMoveEnum.CanNothing);
                    case 2: return new MoveOpportunity(MoveOpportunity.CanMakeMoveEnum.CanAttack);
                    case 11: return new MoveOpportunity(MoveOpportunity.CanMakeMoveEnum.CanThrow);
                    case 12: return new MoveOpportunity(MoveOpportunity.CanMakeMoveEnum.CanThrowAfter);
                    case 21: return new MoveOpportunity(MoveOpportunity.CanMakeMoveEnum.CanDefend);
                    case 22: return new MoveOpportunity(MoveOpportunity.CanMakeMoveEnum.CanNothing);
                    case 10: return new MoveOpportunity(MoveOpportunity.CanMakeMoveEnum.YouLose);
                    case 20: return new MoveOpportunity(MoveOpportunity.CanMakeMoveEnum.YouWin);

                }
            return null;
        }

        private bool ThereIsSuchValueCard(int index, Card NewCard)
        {
            return !(GameRooms[index].CardsOnTable.FindIndex(x => x[0].Value.Equals(NewCard.Value) || x.Count > 1 && x[1].Value.Equals(NewCard.Value)) == -1);
        }

        public bool MakeMove(string RoomName, string password, int PlayerID, Card NewCard, Card TargetCard)
        {
            int index = GetGameRoomAndAuth(RoomName, password, PlayerID);
            if (index == -1) return false;

            var PlayerIsFirst = GameRooms[index].FirstPlayerID == PlayerID;
            var PlayerCards = (PlayerIsFirst) ? GameRooms[index].FirstPlayerCards : GameRooms[index].SecondPlayerCards;
            var OpponentCards = (PlayerIsFirst) ? GameRooms[index].SecondPlayerCards : GameRooms[index].FirstPlayerCards;

            #region Проверка наличия карты
            var CardIndex = (NewCard == null) ? -2 : PlayerCards.FindIndex(x => x.Equals(NewCard));
            if (CardIndex == -1) return false;
            #endregion

            #region Атака
            if (GameRooms[index].GameStatus == 1 && PlayerIsFirst || GameRooms[index].GameStatus == 2 && !PlayerIsFirst) {
                if (PlayerIsFirst)
                    GameRooms[index].FirstPlayerCards.RemoveAt(CardIndex);
                else
                    GameRooms[index].SecondPlayerCards.RemoveAt(CardIndex);

                GameRooms[index].CardsOnTable.Add(new List<Card> { NewCard });
                GameRooms[index].GameStatus = PlayerIsFirst ? 21 : 11;
            }
            #endregion

            #region Защита
            else if (GameRooms[index].GameStatus == 11 && PlayerIsFirst || GameRooms[index].GameStatus == 21 && !PlayerIsFirst) {
                if (NewCard == null && GameRooms[index].UncoverdCardsCount() > 0) {
                    GameRooms[index].GameStatus = PlayerIsFirst ? 12 : 22;
                    return true;
                }

                if (TargetCard == null) return false;

                var TargerCardIndex = GameRooms[index].CardsOnTable.FindIndex(x => x[0].Equals(TargetCard));
                if (TargerCardIndex == -1) return false;

                if ((NewCard.Suit == TargetCard.Suit && NewCard.Value > TargetCard.Value) || (NewCard.Suit == GameRooms[index].TrumpCard.Suit && NewCard.Suit != TargetCard.Suit)) {
                    GameRooms[index].CardsOnTable[TargerCardIndex].Add(NewCard);

                    if (PlayerIsFirst)
                        GameRooms[index].FirstPlayerCards.RemoveAt(CardIndex);
                    else
                        GameRooms[index].SecondPlayerCards.RemoveAt(CardIndex);
                }
            }
            #endregion

            #region Подкидывание
            else if (GameRooms[index].GameStatus == 21 && PlayerIsFirst || GameRooms[index].GameStatus == 11 && !PlayerIsFirst) {
                #region Бито
                if (NewCard == null && GameRooms[index].UncoverdCardsCount() == 0)
                    GameRooms[index].OffTable();
                #endregion

                if (!ThereIsSuchValueCard(index, NewCard) || GameRooms[index].UncoverdCardsCount() >= OpponentCards.Count)
                    return false;

                if (PlayerIsFirst)
                    GameRooms[index].FirstPlayerCards.RemoveAt(CardIndex);
                else
                    GameRooms[index].SecondPlayerCards.RemoveAt(CardIndex);

                GameRooms[index].CardsOnTable.Add(new List<Card> { NewCard });
            }
            #endregion

            #region Подкидывание вдогонку
            else if (GameRooms[index].GameStatus == 22 && PlayerIsFirst || GameRooms[index].GameStatus == 12 && !PlayerIsFirst) {
                if (NewCard == null)
                    GameRooms[index].Take();

                if (!ThereIsSuchValueCard(index, NewCard))
                    return false;

                if (PlayerIsFirst)
                    GameRooms[index].FirstPlayerCards.RemoveAt(CardIndex);
                else
                    GameRooms[index].SecondPlayerCards.RemoveAt(CardIndex);

                GameRooms[index].CardsOnTable.Add(new List<Card> { NewCard });
            }
            #endregion


            if (GameRooms[index].FirstPlayerCards.Count == 0)
                GameRooms[index].GameStatus = 10;
            if (GameRooms[index].SecondPlayerCards.Count == 0)
                GameRooms[index].GameStatus = 20;

            return false;
        }
        #endregion

        #endregion
    }
}
