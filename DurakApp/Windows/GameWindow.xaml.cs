using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DurakApp.DurakServiceReference;

namespace DurakApp.Windows
{
    /// <summary>
    /// Логика взаимодействия для GameWindow.xaml
    /// </summary>
    public partial class GameWindow : Window
    {
        #region Data
        private DurakServiceClient client { get; set; }
        private string RoomName { get; set; }
        private int userID { get; set; }
        private string password { get; set; }
        #endregion

        #region Initialization
        public GameWindow(DurakServiceClient client, string RoomName, int userID, string password)
        {
            this.client = client;
            this.RoomName = RoomName;
            this.userID = userID;
            this.password = password;
            InitializeComponent();

            TestConnect();
        }
        #endregion

        void TestConnect()
        {
            var s = client.GetMyCards(RoomName, password, userID);
            client.GetCardsOnTable(RoomName, password, userID);
            client.GetCardsInStockCount(RoomName, password, userID);
            client.GetOpponentCardsCount(RoomName, password, userID);
            client.GetTrumpCard(RoomName, password, userID);
            client.GetMoveOpportunity(RoomName, password, userID);
            
            client.MakeMove(RoomName, password, userID, s[5], null);
            client.GetMyCards(RoomName, password, userID);
            client.GetCardsOnTable(RoomName, password, userID);
        }
    }
}
