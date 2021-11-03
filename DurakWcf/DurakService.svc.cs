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
            return FreeRooms[index].Password == "*";
        }

        public bool IsFree(string RoomName)
        {
            int index = FreeRooms.FindIndex(x => x.RoomName == RoomName);
            return index != -1;
        }

        public bool ConnectRoom(string RoomName, string password, int PlayerId)
        {
            int index = FreeRooms.FindIndex(x => x.RoomName == RoomName);
            if (index == -1)
                return false;

            FreeRooms[index].Password = password;
            if (FreeRooms[index].Password == "")
            {
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
            if (index != -1) {
                FreeRooms[index].Password = password;
                if (FreeRooms[index].Password == "" && ((FreeRooms[index].FirstPlayerID == PlayerID) || (FreeRooms[index].SecondPlayerID == PlayerID)))
                {
                    FreeRooms.RemoveAt(index);
                    return true;
                }
            }

            index = GameRooms.FindIndex(x => x.RoomName == RoomName);
            if (index != -1)
            {
                GameRooms[index].Password = password;
                if (GameRooms[index].Password == "" && ((GameRooms[index].FirstPlayerID == PlayerID) || (GameRooms[index].SecondPlayerID == PlayerID)))
                {
                    GameRooms.RemoveAt(index);
                    return true;
                }
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
    }
}
