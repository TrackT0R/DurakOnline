using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using DurakApp.DurakServiceReference;
using DurakApp.Windows; 

namespace DurakApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Data
        int userId = 0;
        public string password = "";
        string RoomName = "";
        DurakServiceClient client = new DurakServiceClient();
        #endregion

        #region Initialization
        public MainWindow()
        {
            var rnd = new Random(DateTime.Now.Millisecond);
            userId = rnd.Next(100000, 999999);
            InitializeComponent();
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Start();
            timer_Tick(new object(), new EventArgs());
        }

        private DispatcherTimer timer = new DispatcherTimer();

        private void timer_Tick(object sender, EventArgs e)
        {
            RefreshButton_Click(sender, new RoutedEventArgs());
        }
        #endregion

        #region Refresh list
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            var t = Listbox.SelectedIndex;
            var rooms = client.GetFreeRooms();
            Listbox.Items.Clear();
            foreach (var room in rooms)
            {
                Listbox.Items.Add(new ListBoxItem() { Content = $"{room} {new string(' ', 25 - room.Length)} {(client.HasPassword(room) ? "🔒" : "  ")}" });
            }
            Listbox.SelectedIndex = t;
        }
        #endregion

        #region Create room
        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (RoomName != "")
                client.DeleteRoom(RoomName, password, userId);

            var rc = new RoomCreateWindow(client, userId);
            rc.Left = this.Left + 50;
            rc.Top = this.Top + 50;
            if (!rc.ShowDialog() == true)
                return;

            timer_Tick(new object(), new EventArgs());

            password = rc.PasswordBox.Password;
            RoomName = rc.RoomNameTextBox.Text;

            var ww = new WaitWindow(client, RoomName);
            ww.Left = this.Left + 50;
            ww.Top = this.Top + 50;

            if (ww.ShowDialog() == true)
            {
                this.Visibility = Visibility.Hidden;
                
                GameStart();
            }
            
            client.DeleteRoom(RoomName, password, userId);
            timer_Tick(new object(), new EventArgs());
        }
        #endregion

        #region Connect to room
        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (Listbox.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите комнату!");
                return;
            }
            var s = Listbox.Items[Listbox.SelectedIndex].ToString();
            RoomName = s.Substring(37, 27).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];

            if (s[64] != ' ')
            {
                var pw = new PasswordWindow();
                pw.Left = this.Left + 50;
                pw.Top = this.Top + 50;
                if (pw.ShowDialog() == true)
                    if (!client.ConnectRoom(RoomName, pw.PasswordBox.Password, userId))
                    {
                        MessageBox.Show("Не удалось подключиться");
                        return;
                    }
            }
            else
            {
                if (!client.ConnectRoom(RoomName, "", userId))
                {
                    MessageBox.Show("Не удалось подключиться");
                    return;
                }
            }
            
            GameStart();
        }
        #endregion

        #region Start game
        private void GameStart()
        {
            this.Visibility = Visibility.Hidden;
            var gw = new GameWindow(client, RoomName, userId, password);
            gw.Left = this.Left + 50;
            gw.Top = 0;
            gw.ShowDialog();
            client.DeleteRoom(RoomName, password, userId);
            this.Visibility = Visibility.Visible;
        }
        #endregion

        #region Service methods
        private void Listbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Listbox.SelectedIndex != -1)
                ConnectButton.Visibility = Visibility.Visible;
            else
                ConnectButton.Visibility = Visibility.Hidden;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            client.DeleteRoom(RoomName, password, userId);
            client.Close();
            Application.Current.Shutdown();
        }
        #endregion
    }
}
