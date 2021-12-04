using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace DurakWcf
{
    // ПРИМЕЧАНИЕ. Команду "Переименовать" в меню "Рефакторинг" можно использовать для одновременного изменения имени интерфейса "IService1" в коде и файле конфигурации.
    [ServiceContract]
    public interface IDurakService
    {
        #region Room methods
        [OperationContract]
        bool HasPassword(string RoomName);

        [OperationContract]
        bool CreateRoom(string RoomName, string password, int UserID);

        [OperationContract]
        bool DeleteRoom(string RoomName, string password, int UserID);

        [OperationContract]
        bool ConnectRoom(string RoomName, string password, int UserID);

        [OperationContract]
        string[] GetFreeRooms();
        #endregion

        #region Game methods
        [OperationContract]
        List<Card> GetMyCards(string RoomName, string password, int PlayerID);
        [OperationContract]
        List<List<Card>> GetCardsOnTable(string RoomName, string password, int PlayerID);
        [OperationContract]
        int GetCardsInStockCount(string RoomName, string password, int PlayerID);
        [OperationContract]
        int GetOpponentCardsCount(string RoomName, string password, int PlayerID);
        [OperationContract]
        Card GetTrumpCard(string RoomName, string password, int PlayerID);
        [OperationContract]
        MoveOpportunity GetMoveOpportunity(string RoomName, string password, int PlayerID);
        [OperationContract]
        bool MakeMove(string RoomName, string password, int PlayerID, Card NewCard, Card TargetCard);
        [OperationContract]
        Card Card(Card.SuitEnum suit, Card.ValueEnum value);
        #endregion
        // TODO: Добавьте здесь операции служб
    }


    // Используйте контракт данных, как показано в примере ниже, чтобы добавить составные типы к операциям служб.
    [DataContract]
    public class Room
    {
        #region Room data
        public int FirstPlayerID { get; private set; }
        public int SecondPlayerID { get; private set; }
        public string RoomName { get; private set; }
        public string password { get; private set; }
        #endregion

        #region Game data
        public int GameStatus { get; set; } = 0;
        public Card TrumpCard { get; private set; }
        public List<Card> FirstPlayerCards { get; private set; } = new List<Card>();
        public List<Card> SecondPlayerCards { get; private set; } = new List<Card>();
        public List<Card> CardsInStock { get; private set; } = new List<Card>();
        public List<List<Card>> CardsOnTable { get; private set; } = new List<List<Card>>();
        #endregion

        #region Constructors
        public Room(string RoomName, string password, int FirstPlayerID)
        {
            this.RoomName = RoomName;
            this.password = password;
            this.FirstPlayerID = FirstPlayerID;
        }
        #endregion

        #region Methods
        public bool AddSecondPlayerID(int SecondPlayerID)
        {
            if (FirstPlayerID == SecondPlayerID)
                return false;
            this.SecondPlayerID = SecondPlayerID;

            BeginCardDraw();

            return true;
        }

        private void BeginCardDraw()
        {
            CardsOnTable = new List<List<Card>>();

            #region Размешивание колоды
            var rnd = new Random();
            foreach (Card.SuitEnum suit in Enum.GetValues(typeof(Card.SuitEnum)))
                foreach (Card.ValueEnum value in Enum.GetValues(typeof(Card.ValueEnum)))
                    CardsInStock.Insert(rnd.Next(CardsInStock.Count + 1), new Card(suit, value));
            #endregion

            #region Выдача карт, определение козыря, отправка козыря вниз колоды
            FirstPlayerCards.AddRange(CardsInStock.GetRange(0, 6));
            SecondPlayerCards.AddRange(CardsInStock.GetRange(6, 6));
            TrumpCard = CardsInStock[12];
            CardsInStock.RemoveRange(0, 13);
            CardsInStock.Add(TrumpCard);

            FirstPlayerCards.Sort();
            SecondPlayerCards.Sort();
            #endregion

            #region Определение очерёдности хода
            var firstPlayerTrumps = FirstPlayerCards.FindAll(x => x.Suit == TrumpCard.Suit);
            var secondPlayerTrumps = SecondPlayerCards.FindAll(x => x.Suit == TrumpCard.Suit);

            if (firstPlayerTrumps.Count > 0)
                if (secondPlayerTrumps.Count > 0)
                    if (firstPlayerTrumps.Min().Value < secondPlayerTrumps.Min().Value)
                        GameStatus = 1;
                    else
                        GameStatus = 2;
                else
                    GameStatus = 1;
            else
                if (secondPlayerTrumps.Count > 0)
                GameStatus = 2;
            else
                GameStatus = rnd.Next(1, 3);
            #endregion
        }

        public int UncoverdCardsCount()
        {
            int cnt = 0;
            foreach (var c in CardsOnTable) {
                if (c.Count == 1)
                    cnt++;
            }
            return cnt;
        }

        private void GiveCardsToFirst()
        {
            while (FirstPlayerCards.Count < 6) {
                if (CardsInStock.Count == 0)
                    return;
                FirstPlayerCards.Add(CardsInStock[0]);
                CardsInStock.RemoveAt(0);
            }
        }

        private void GiveCardsToSecond()
        {
            while (SecondPlayerCards.Count < 6) {
                if (CardsInStock.Count == 0)
                    return;
                SecondPlayerCards.Add(CardsInStock[0]);
                CardsInStock.RemoveAt(0);
            }
        }

        public void OffTable()
        {
            CardsOnTable = new List<List<Card>>();
            
            if (GameStatus == 11) {
                GameStatus = 1;
                GiveCardsToFirst();
                GiveCardsToSecond();
            }
            else {
                GameStatus = 2;
                GiveCardsToSecond();
                GiveCardsToFirst();
            }

            FirstPlayerCards.Sort();
            SecondPlayerCards.Sort();
        }

        public void Take()
        {
            foreach (var c in CardsOnTable)
                if (GameStatus == 12)
                    FirstPlayerCards.AddRange(c);
                else
                    SecondPlayerCards.AddRange(c);

            CardsOnTable = new List<List<Card>>();
            
            if (GameStatus == 12) {
                GameStatus = 2;
                GiveCardsToSecond();
            }
            else {
                GameStatus = 1;
                GiveCardsToFirst();
            }
        }
        #endregion
    }


    [DataContract]
    public class Card : IComparable<Card>
    {
        public enum SuitEnum { heart, diamond, club, spade }
        public enum ValueEnum { six, seven, eight, nine, ten, jack, queen, king, ace }

        [DataMember]
        public SuitEnum Suit { get; private set; }
        [DataMember]
        public ValueEnum Value { get; private set; }

        #region Methods
        public Card(SuitEnum suit, ValueEnum value)
        {
            Suit = suit;
            Value = value;
        }

        public int CompareTo(Card other)
        {
            return this.Suit.CompareTo(other.Suit) != 0 ? this.Suit.CompareTo(other.Suit) : this.Value.CompareTo(other.Value);
        }

        public override bool Equals(object obj) => this.Equals(obj as Card);

        public bool Equals(Card c)
        {
            if (this == null && c == null) return false;
            return Suit.Equals(c.Suit) && Value.Equals(c.Value);
        }

        public override int GetHashCode()
        {
            var hashCode = -1625629942;
            hashCode = hashCode * -1521134295 + Suit.GetHashCode();
            hashCode = hashCode * -1521134295 + Value.GetHashCode();
            return hashCode;
        }
        #endregion
    }

    [DataContract]
    public class MoveOpportunity
    {
        public enum CanMakeMoveEnum { CanAttack, CanThrow, CanThrowAfter, CanDefend, CanNothing, YouWin, YouLose }
        [DataMember]
        public CanMakeMoveEnum CanMakeMove { get; private set; }

        public MoveOpportunity(CanMakeMoveEnum CanMakeMove)
        {
            this.CanMakeMove = CanMakeMove;
        }

        public override bool Equals(object obj) => this.Equals(obj as MoveOpportunity);

        public bool Equals(MoveOpportunity m)
        {
            return CanMakeMove.Equals(m.CanMakeMove);
        }

        public override int GetHashCode()
        {
            return -1714908010 + CanMakeMove.GetHashCode();
        }
    }
}
