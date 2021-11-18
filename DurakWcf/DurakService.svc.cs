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
        private static List<Room> FreeRooms = new List<Room>();
        private static List<Room> GameRooms = new List<Room>();

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

        #region Cards info requests
        public List<Card> GetMyCards(string RoomName, string password, int PlayerID)
        {
            int index = GameRooms.FindIndex(x => x.RoomName == RoomName);
            if (index == -1 || GameRooms[index].password != password ||
                (PlayerID != GameRooms[index].FirstPlayerID && PlayerID != GameRooms[index].SecondPlayerID))
                return null;

            if (GameRooms[index].FirstPlayerID == PlayerID)
                return GameRooms[index].FirstPlayerCards;
            else
                return GameRooms[index].SecondPlayerCards;
        }

        public List<List<Card>> GetCardsOnTable(string RoomName, string password, int PlayerID)
        {
            int index = GameRooms.FindIndex(x => x.RoomName == RoomName);
            if (index == -1 || GameRooms[index].password != password ||
                (PlayerID != GameRooms[index].FirstPlayerID && PlayerID != GameRooms[index].SecondPlayerID))
                return null;

            return GameRooms[index].CardsOnTable;
        }

        public int GetCardsInStockCount(string RoomName, string password, int PlayerID)
        {
            int index = GameRooms.FindIndex(x => x.RoomName == RoomName);
            if (index == -1 || GameRooms[index].password != password ||
                (PlayerID != GameRooms[index].FirstPlayerID && PlayerID != GameRooms[index].SecondPlayerID))
                return -1;

            return GameRooms[index].CardsInStock.Count;
        }

        public int GetOpponentCardsCount(string RoomName, string password, int PlayerID)
        {
            int index = GameRooms.FindIndex(x => x.RoomName == RoomName);
            if (index == -1 || GameRooms[index].password != password ||
                (PlayerID != GameRooms[index].FirstPlayerID && PlayerID != GameRooms[index].SecondPlayerID))
                return -1;

            if (GameRooms[index].FirstPlayerID == PlayerID)
                return GameRooms[index].SecondPlayerCards.Count;
            else
                return GameRooms[index].FirstPlayerCards.Count;
        }

        public Card GetTrumpCard(string RoomName, string password, int PlayerID)
        {
            int index = GameRooms.FindIndex(x => x.RoomName == RoomName);
            if (index == -1 || GameRooms[index].password != password ||
                (PlayerID != GameRooms[index].FirstPlayerID && PlayerID != GameRooms[index].SecondPlayerID))
                return null;

            return GameRooms[index].TrumpCard;
        }
        #endregion

        #region Move processing
        public MoveOpportunity GetMoveOpportunity(string RoomName, string password, int PlayerID)
        {
            int index = GameRooms.FindIndex(x => x.RoomName == RoomName);
            if (index == -1 || GameRooms[index].password != password ||
                (PlayerID != GameRooms[index].FirstPlayerID && PlayerID != GameRooms[index].SecondPlayerID))
                return null;

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

        public bool MakeMove(string RoomName, string password, int PlayerID, Card NewCard, Card TargetCard)
        {
            int index = GameRooms.FindIndex(x => x.RoomName == RoomName);
            if (index == -1 || GameRooms[index].password != password || (PlayerID != GameRooms[index].FirstPlayerID && PlayerID != GameRooms[index].SecondPlayerID))
                return false;

            if (GameRooms[index].FirstPlayerID == PlayerID) {
                #region Проверка наличия карты
                var CardIndex = (NewCard == null) ? -2 : GameRooms[index].FirstPlayerCards.FindIndex(x => x.Equals(NewCard));
                if (CardIndex == -1)
                    return false;
                #endregion

                switch (GameRooms[index].GameStatus) {
                    #region Атака
                    case 1:
                        GameRooms[index].FirstPlayerCards.RemoveAt(CardIndex);
                        GameRooms[index].CardsOnTable.Add(new List<Card> { NewCard });
                        GameRooms[index].GameStatus = 21;
                        break;
                    #endregion
                    #region Защита
                    case 11:
                        if (NewCard == null) {
                            GameRooms[index].GameStatus = 12;
                            return true;
                        }

                        if (TargetCard == null)
                            return false;

                        var TargerCardIndex = GameRooms[index].CardsOnTable.FindIndex(x => x[0].Equals(TargetCard));
                        if (TargerCardIndex == -1)
                            return false;

                        if ((NewCard.Suit == TargetCard.Suit && NewCard.Value > TargetCard.Value) || (NewCard.Suit == GameRooms[index].TrumpCard.Suit && NewCard.Suit != TargetCard.Suit)) {
                            GameRooms[index].CardsOnTable[TargerCardIndex].Add(NewCard);
                            GameRooms[index].FirstPlayerCards.RemoveAt(CardIndex);
                        }
                        break;
                    #endregion
                    #region Подкидывание
                    case 21:
                        #region Бито
                        if (NewCard == null && GameRooms[index].UncoverdCardsCount() == 0) {
                            GameRooms[index].OffTable();
                        }
                        #endregion

                        if (GameRooms[index].CardsOnTable.FindIndex(x => x[0].Value.Equals(NewCard.Value)) == -1 || GameRooms[index].UncoverdCardsCount() >= GameRooms[index].SecondPlayerCards.Count)
                            return false;

                        GameRooms[index].FirstPlayerCards.RemoveAt(CardIndex);
                        GameRooms[index].CardsOnTable.Add(new List<Card> { NewCard });
                        break;
                    #endregion
                    #region Подкидывание вдогонку
                    case 22:
                        if (NewCard == null)
                            GameRooms[index].Take();

                        if (GameRooms[index].CardsOnTable.FindIndex(x => x[0].Value.Equals(NewCard.Value)) == -1)
                            return false;

                        GameRooms[index].FirstPlayerCards.RemoveAt(CardIndex);
                        GameRooms[index].CardsOnTable.Add(new List<Card> { NewCard });
                        break;
                        #endregion
                }
            }
            else {
                #region Проверка наличия карты
                var CardIndex = (NewCard == null) ? -2 : GameRooms[index].SecondPlayerCards.FindIndex(x => x.Equals(NewCard));
                if (CardIndex == -1)
                    return false;
                #endregion
                switch (GameRooms[index].GameStatus) {
                    #region Атака
                    case 2:
                        GameRooms[index].SecondPlayerCards.RemoveAt(CardIndex);
                        GameRooms[index].CardsOnTable.Add(new List<Card> { NewCard });
                        GameRooms[index].GameStatus = 11;
                        break;
                    #endregion
                    #region Защита
                    case 21:
                        if (NewCard == null) {
                            GameRooms[index].GameStatus = 22;
                            return true;
                        }

                        if (TargetCard == null)
                            return false;

                        var TargerCardIndex = GameRooms[index].CardsOnTable.FindIndex(x => x[0].Equals(TargetCard));
                        if (TargerCardIndex == -1)
                            return false;

                        if ((NewCard.Suit == TargetCard.Suit && NewCard.Value > TargetCard.Value) || (NewCard.Suit == GameRooms[index].TrumpCard.Suit && NewCard.Suit != TargetCard.Suit)) {
                            GameRooms[index].CardsOnTable[TargerCardIndex].Add(NewCard);
                            GameRooms[index].SecondPlayerCards.RemoveAt(CardIndex);
                        }
                        break;
                    #endregion
                    #region Подкидывание
                    case 11:
                        #region Бито
                        if (NewCard == null && GameRooms[index].UncoverdCardsCount() == 0) {
                            GameRooms[index].OffTable();
                        }
                        #endregion

                        var TargerCardInd = GameRooms[index].CardsOnTable.FindIndex(x => x[0].Value.Equals(NewCard.Value));
                        if (TargerCardInd == -1 || GameRooms[index].UncoverdCardsCount() >= GameRooms[index].FirstPlayerCards.Count)
                            return false;

                        GameRooms[index].SecondPlayerCards.RemoveAt(CardIndex);
                        GameRooms[index].CardsOnTable.Add(new List<Card> { NewCard });
                        break;
                    #endregion
                    #region Подкидывание вдогонку
                    case 12:
                        if (NewCard == null)
                            GameRooms[index].Take();

                        if (GameRooms[index].CardsOnTable.FindIndex(x => x[0].Value.Equals(NewCard.Value)) == -1)
                            return false;

                        GameRooms[index].SecondPlayerCards.RemoveAt(CardIndex);
                        GameRooms[index].CardsOnTable.Add(new List<Card> { NewCard });
                        break;
                        #endregion
                }
            }

            if (GameRooms[index].FirstPlayerCards.Count == 0)
                GameRooms[index].GameStatus = 10;
            if (GameRooms[index].SecondPlayerCards.Count == 0)
                GameRooms[index].GameStatus = 20;

            return false;
        }
        #endregion

        #endregion

        public Card Card(Card.SuitEnum suit, Card.ValueEnum value)
        {
            return new Card(suit, value);
        }
    }
}
