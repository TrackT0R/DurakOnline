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
        [OperationContract]
        bool HasPassword(string RoomName);

        [OperationContract]
        bool IsFree(string RoomName);

        [OperationContract]
        bool CreateRoom(string RoomName, string password, int UserID);

        [OperationContract]
        bool DeleteRoom(string RoomName, string password, int UserID);

        [OperationContract]
        bool ConnectRoom(string RoomName, string password, int UserID);

        [OperationContract]
        string[] GetFreeRooms();
        
        // TODO: Добавьте здесь операции служб
    }


    // Используйте контракт данных, как показано в примере ниже, чтобы добавить составные типы к операциям служб.
    [DataContract]
    public class Room
    {
        #region Data
        [DataMember]
        public int FirstPlayerID  { get; private set; }
        [DataMember]
        public int SecondPlayerID { get; private set; }
        [DataMember]
        public string RoomName    { get; private set; }
        [DataMember]
        public bool IsFree        { get; private set; }
        [DataMember]
        private string password   { get; set; }
        #endregion

        #region Constructors
        public Room(string RoomName, string password, int FirstPlayerID)
        {
            this.RoomName = RoomName;
            this.password = password;
            this.FirstPlayerID = FirstPlayerID;
            IsFree = true;
        }
        #endregion

        #region Methods
        public bool AddSecondPlayerID(int SecondPlayerID)
        {
            if (FirstPlayerID == SecondPlayerID)
                return false;
            this.SecondPlayerID = SecondPlayerID;
            IsFree = false;
            return true;
        }

        public string Password
        {
            get
            {
                return password.Length > 0 ? "*" : "";
            }
            set
            {
                if (password == value)
                    password = "";
            }
        }
        #endregion
    }

    [DataContract]
    public class Game
    {

    }
}
