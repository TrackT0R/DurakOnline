using System.Windows;
using System.Windows.Controls;
using DurakApp.DurakServiceReference;

namespace DurakApp
{
    /// <summary>
    /// Логика взаимодействия для RoomCreateWindow.xaml
    /// </summary>
    public partial class RoomCreateWindow : Window
    {
        #region Data
        private DurakServiceClient client;
        private int userID;
        #endregion

        #region Initialization
        public RoomCreateWindow(DurakServiceClient client, int userID)
        {
            this.client = client;
            this.userID = userID;
            InitializeComponent();
            RoomNameTextBox.Focus();
        }
        #endregion

        #region Create room
        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (RoomNameTextBox.Text.Contains(" "))
                MessageBox.Show("Имя не должно содержать пробелы");
            else if (!client.CreateRoom(RoomNameTextBox.Text, PasswordBox.Password, userID))
                MessageBox.Show("Комнату создать не удалось");
            else
                this.DialogResult = true;
        }
        #endregion
    }
}
