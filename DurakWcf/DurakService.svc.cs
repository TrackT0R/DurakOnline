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
                if (FreeRooms[index].password == password && ((FreeRooms[index].FirstPlayerID == PlayerID) || (FreeRooms[index].SecondPlayerID == PlayerID)))
                {
                    FreeRooms.RemoveAt(index);
                    return true;
                }
            
            index = GameRooms.FindIndex(x => x.RoomName == RoomName);
            if (index != -1)
                if (GameRooms[index].password == password && ((GameRooms[index].FirstPlayerID == PlayerID) || (GameRooms[index].SecondPlayerID == PlayerID)))
                {
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

        public MoveOpportunity GetMoveOpportunity(string RoomName, string password, int PlayerID)
        {
            int index = GameRooms.FindIndex(x => x.RoomName == RoomName);
            if (index == -1 || GameRooms[index].password != password ||
                (PlayerID != GameRooms[index].FirstPlayerID && PlayerID != GameRooms[index].SecondPlayerID))
                return null;

            if (GameRooms[index].FirstPlayerID == PlayerID)
                switch (GameRooms[index].GameStatus) {
                    case 1 : return new MoveOpportunity(MoveOpportunity.CanMakeMoveEnum.CanAttack);
                    case 2 : return new MoveOpportunity(MoveOpportunity.CanMakeMoveEnum.CanNothing);
                    case 11 : return new MoveOpportunity(MoveOpportunity.CanMakeMoveEnum.CanDefend);
                    case 22 : return new MoveOpportunity(MoveOpportunity.CanMakeMoveEnum.CanThrow);
                }
            else
                switch (GameRooms[index].GameStatus) {
                    case 11: return new MoveOpportunity(MoveOpportunity.CanMakeMoveEnum.CanAttack);
                    case 22: return new MoveOpportunity(MoveOpportunity.CanMakeMoveEnum.CanNothing);
                    case 1: return new MoveOpportunity(MoveOpportunity.CanMakeMoveEnum.CanDefend);
                    case 2: return new MoveOpportunity(MoveOpportunity.CanMakeMoveEnum.CanThrow);
                }
            return null;

        }

        public bool MakeMove(string RoomName, string password, int PlayerID, Card NewCard, Card TargetCard)
        {
            int index = GameRooms.FindIndex(x => x.RoomName == RoomName);
            if (index == -1 || GameRooms[index].password != password ||
                (PlayerID != GameRooms[index].FirstPlayerID && PlayerID != GameRooms[index].SecondPlayerID))
                return false;

            if (GameRooms[index].FirstPlayerID == PlayerID) {
                if (GameRooms[index].GameStatus == 1) {
                    GameRooms[index].FirstPlayerCards.Remove(NewCard);
                    GameRooms[index].CardsOnTable.Add(new List<Card> {NewCard});
                    return true;
                }
            }
            else {
                if (GameRooms[index].GameStatus == 2) {
                    GameRooms[index].SecondPlayerCards.Remove(NewCard);
                    GameRooms[index].CardsOnTable.Add(new List<Card> { NewCard });
                    return true;
                }
            }
            return false;
        }

        public Card Card(Card.SuitEnum suit, Card.ValueEnum value)
        {
            return new Card(suit, value);
        }
    }
}
