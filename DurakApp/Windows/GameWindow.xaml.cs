using DurakApp.DurakServiceReference;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

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

        private DispatcherTimer timer = new DispatcherTimer();
        string first = ""; // эта штука понадобится, объяснения будут ниже
        string second = ""; // тоже понадобится
        Button firstClicked = null; // Это ссылочная переменная!!!!!!!!! Она не добавляет новую кнопку в wpf, т.к не использует ключевое слово new и объект тем самым не создается
        Button secondClicked = null; // Тоже ссылочная переменная. Они будут ссылаться на наши кнопки, на которые мы кликаем

        #endregion
        
        #region Initialization
        public GameWindow(DurakServiceClient client, string RoomName, int userID, string password)
        {
            this.client = client;
            this.RoomName = RoomName;
            this.userID = userID;
            this.password = password;
            InitializeComponent();
            TrumpButton.Source = new BitmapImage(new Uri(@"pack://application:,,,/Cards/" + client.GetTrumpCard(RoomName, password, userID).Suit.ToString() + @"/" + client.GetTrumpCard(RoomName, password, userID).Value.ToString() + ".jpg"));
            Rubashka.Source = new BitmapImage(new Uri(@"pack://application:,,,/images/Rubashka.jpg"));
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Start();

        }
        #endregion

        
        #region MouseDown
        private void Button_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Button clickButton = sender as Button; //эта строка преобразует переменную sender в метку с именем clickButton
            string GetMoveOpportunity = client.GetMoveOpportunity(RoomName, password, userID).CanMakeMove.ToString();
            Card[][] GetCardsOnTable = client.GetCardsOnTable(RoomName, password, userID);
            Card[] GetMyCards = client.GetMyCards(RoomName, password, userID);
            int GetOpponentCardsCount = client.GetOpponentCardsCount(RoomName, password, userID);
            if (clickButton != null) {
                if (firstClicked == null) {
                    firstClicked = clickButton;
                    first = firstClicked.Name.ToString();
                    timer.Stop();
                    BitoButton.IsEnabled = false;
                    TakeButton.IsEnabled = false;
                    HandGrid.IsEnabled = false;
                    if (GetMoveOpportunity == "CanNothing" || GetMoveOpportunity == "CanDefend")
                    {
                        int l = 0;
                        TableGrid.IsEnabled = true;
                        foreach(var button in TableGrid.Children.OfType<Button>())
                        {
                            if (button.Name == "")
                                button.IsEnabled = false;
                            int j = 0;
                            foreach (var button1 in TableGrid2.Children.OfType<Button>())
                            {
                                if (j == l && button1.Name != "")
                                {
                                    button.IsEnabled = false;
                                    break;
                                }
                                j++;
                            }
                            l++;
                        }
                    }
                    if (GetMoveOpportunity == "CanAttack" || GetMoveOpportunity == "CanThrow" || GetMoveOpportunity == "CanThrowAfter")
                    {
                        TableGrid2.IsEnabled = true;
                        foreach (var button in TableGrid2.Children.OfType<Button>())
                        {
                            if (button.Name != "")
                                button.IsEnabled = false;
                        }
                    }
                    return;
                }
                if (secondClicked == null) {
                    secondClicked = clickButton;
                    second = secondClicked.Name.ToString();
                    string[] s1 = first.Split('_');
                    string[] s2 = second.Split('_');
                    int i1 = -1;
                    int i = 0;
                    if (s1.Length == 2) {
                        foreach (var card in GetMyCards) {
                            if (card.Suit.ToString() == s1[0] && card.Value.ToString() == s1[1]) {
                                i1 = i;
                                break;
                            }
                            i++;
                        }
                    }
                    int b2 = 0;
                    int b1 = 0;
                    int j2 = 0;
                    int j1 = 0;
                    bool f = false;
                    if (s2.Length == 2) {
                        foreach (var list in GetCardsOnTable) {
                            foreach (var card in list) {
                                if (card.Suit.ToString() == s2[0] && card.Value.ToString() == s2[1]) {
                                    b1 = j1;
                                    b2 = j2;
                                    f = true;
                                    break;
                                }
                            }
                            if (f)
                                break;
                            j1++;
                            j2 = 0;
                        }
                    }
                    
                    if (s2.Length < 2)
                        client.MakeMove(RoomName, password, userID, GetMyCards[i1], null);
                    else
                        client.MakeMove(RoomName, password, userID, GetMyCards[i1], GetCardsOnTable[b1][b2]);
                    first = "";
                    firstClicked = null;
                    second = "";
                    secondClicked = null;
                    HandGrid.IsEnabled = true;
                    timer.Start();
                    RefreshButton_Click(sender, new RoutedEventArgs());
                    return;

                }
            }//проверяет произошло ли успешное преобразование из строки выше, если не null то всё ок, если null то это бы означало что мы нажали не на кнопку, а на чето другое
        }//обрабатывает событие клика
        #endregion

        #region Cards Refresh
        private void timer_Tick(object sender, EventArgs e)
        {
            RefreshButton_Click(sender, new RoutedEventArgs());
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            string GetMoveOpportunity = "err";
            Card[][] GetCardsOnTable = null;
            Card[] GetMyCards = null;
            int GetOpponentCardsCount = -1;
            int GetCardsInStockCount = -1;
            try {
                GetMoveOpportunity = client.GetMoveOpportunity(RoomName, password, userID).CanMakeMove.ToString();
                GetCardsOnTable = client.GetCardsOnTable(RoomName, password, userID);
                GetMyCards = client.GetMyCards(RoomName, password, userID);
                GetOpponentCardsCount = client.GetOpponentCardsCount(RoomName, password, userID);
                GetCardsInStockCount = client.GetCardsInStockCount(RoomName, password, userID);
            }
            catch {}

            if (GetMoveOpportunity == "YouWin" || GetMoveOpportunity == "YouLose") {
                timer.Stop();
                MessageBox.Show(GetMoveOpportunity);
                //this.Close();
                DialogResult = true;
                return;
            }

            if (GetMoveOpportunity == "err" || GetCardsOnTable == null || GetMyCards == null || GetOpponentCardsCount == -1 || GetCardsInStockCount == -1) {
                timer.Stop();
                MessageBox.Show("Other player is out of the game");
                DialogResult = true;
                //this.Close();
                return;
            }

            TableGrid.ColumnDefinitions.Clear();
            TableGrid2.ColumnDefinitions.Clear();
            TableGrid.Children.Clear();
            TableGrid2.Children.Clear();
            ColumnDefinition cd;
            for (int j = 0; j < GetCardsOnTable.Count() + 1; j++)
            {
                cd = new ColumnDefinition();
                TableGrid.ColumnDefinitions.Add(cd);
            }

            for (int j = 0; j < GetCardsOnTable.Count() + 1; j++)
            {
                cd = new ColumnDefinition();
                TableGrid2.ColumnDefinitions.Add(cd);
            }

            Button nb;
            for (int j = 0; j < GetCardsOnTable.Count() + 1; j++)
            {
                nb = new Button();
                nb.PreviewMouseLeftButtonDown += Button_MouseDown;
                nb.MouseEnter += Enter_Button;
                nb.MouseLeave += Leave_Button;
                TableGrid.Children.Add(nb);
                Grid.SetColumn(nb, j);
            }

            for (int j = 0; j < GetCardsOnTable.Count() + 1; j++)
            {
                nb = new Button();
                nb.PreviewMouseLeftButtonDown += Button_MouseDown;
                nb.MouseEnter += Enter_Button;
                nb.MouseLeave += Leave_Button;
                TableGrid2.Children.Add(nb);
                Grid.SetColumn(nb, j);
            }


            int i = 0;
            OponentCardsCount.Content = GetOpponentCardsCount;
            StockButton.Content = GetCardsInStockCount;
            TestButton.Content = GetMoveOpportunity;           
            foreach (var button in TableGrid.Children.OfType<Button>())//Чистка карт на столе
            {
                button.Name = "";              
                button.Content = "";
            }
            foreach (var button in TableGrid2.Children.OfType<Button>())//Чистка карт на столе
            {
                button.Name = "";               
                button.Content = "";
            }

            if (GetMoveOpportunity == "CanNothing" || GetMoveOpportunity == "CanDefend") {

                BitoButton.Visibility = Visibility.Hidden;
                TakeButton.Visibility = Visibility.Visible;
                TableGrid2.IsEnabled = false;
                TableGrid.IsEnabled = false;
                bool b = true;
                foreach (var list in GetCardsOnTable)
                    if (list.Count() != 2)
                    {
                        b = false;
                        break;
                    }
                if (b || GetMoveOpportunity == "CanNothing")
                {
                    HandGrid.IsEnabled = false;
                    TakeButton.IsEnabled = false;
                }
                else
                {
                    HandGrid.IsEnabled = true;
                    TakeButton.IsEnabled = true;
                }

                foreach (var button in TableGrid.Children.OfType<Button>()) {
                    if (i > GetCardsOnTable.Count() - 1) {
                        i = 0;
                        break;
                    }
                    button.Name = GetCardsOnTable[i][0].Suit + "_" + GetCardsOnTable[i][0].Value;
                    string str = @"pack://application:,,,/Cards/" + GetCardsOnTable[i][0].Suit + @"/" + GetCardsOnTable[i][0].Value + ".jpg";
                    Image image = new Image(); //чтобы присваивать изображения кнопкам
                    image.Source = new BitmapImage(new Uri(str));
                    button.Content = image; 
                    i++;
                }
                foreach (var button in TableGrid2.Children.OfType<Button>()) {
                    if (i > GetCardsOnTable.Count() - 1){
                        i = 0;
                        break;
                    }
                    if (GetCardsOnTable[i].Count() < 2)
                    {
                        i++;
                        continue;
                    }
                    button.Name = GetCardsOnTable[i][1].Suit + "_" + GetCardsOnTable[i][1].Value;
                    string str = @"pack://application:,,,/Cards/" + GetCardsOnTable[i][1].Suit + @"/" + GetCardsOnTable[i][1].Value + ".jpg";
                    Image image = new Image(); //чтобы присваивать изображения кнопкам
                    image.Source = new BitmapImage(new Uri(str));
                    button.Content = image;
                    i++;
                }
            }
           
            

            if (GetMoveOpportunity == "CanAttack" || GetMoveOpportunity == "CanThrow" || GetMoveOpportunity == "CanThrowAfter") {
                TakeButton.Visibility = Visibility.Hidden;
                BitoButton.Visibility = Visibility.Visible;
                TableGrid.IsEnabled = false;
                TableGrid2.IsEnabled = false;
                HandGrid.IsEnabled = true;
                bool b = true;
                foreach (var list in GetCardsOnTable)
                    if (list.Count() < 2)
                    {
                        b = false;
                        break;
                    }
                if ((!b || GetMoveOpportunity == "CanAttack") && !(GetMoveOpportunity == "CanThrowAfter"))
                    BitoButton.IsEnabled = false;
                else
                    BitoButton.IsEnabled = true;             
                foreach (var button in TableGrid2.Children.OfType<Button>()) {
                    if (i > GetCardsOnTable.Count() - 1) {
                        i = 0;
                        break;
                    }
                    button.Name = GetCardsOnTable[i][0].Suit + "_" + GetCardsOnTable[i][0].Value;
                    string str = @"pack://application:,,,/Cards/" + GetCardsOnTable[i][0].Suit + @"/" + GetCardsOnTable[i][0].Value + ".jpg";
                    Image image = new Image(); //чтобы присваивать изображения кнопкам
                    image.Source = new BitmapImage(new Uri(str));
                    button.Content = image;
                    i++;
                }
                foreach (var button in TableGrid.Children.OfType<Button>()) {
                    if (i > GetCardsOnTable.Count() - 1 ) {
                        i = 0;
                        break;
                    }
                    if (GetCardsOnTable[i].Count() < 2)
                    {
                        i++;
                        continue;
                    }
                    button.Name = GetCardsOnTable[i][1].Suit + "_" + GetCardsOnTable[i][1].Value;
                    string str = @"pack://application:,,,/Cards/" + GetCardsOnTable[i][1].Suit + @"/" + GetCardsOnTable[i][1].Value + ".jpg";
                    Image image = new Image(); //чтобы присваивать изображения кнопкам
                    image.Source = new BitmapImage(new Uri(str));
                    button.Content = image;
                    i++;
                }
            }

            HandGrid.ColumnDefinitions.Clear();
            HandGrid.Children.Clear();
            OpponentHandGrid.ColumnDefinitions.Clear();
            OpponentHandGrid.Children.Clear();
            for (int j = 0; j < GetMyCards.Count(); j++)
            {
                cd = new ColumnDefinition();
                HandGrid.ColumnDefinitions.Add(cd);
            }

            for (int j = 0; j < GetOpponentCardsCount; j++)
            {
                cd = new ColumnDefinition();
                OpponentHandGrid.ColumnDefinitions.Add(cd);
            }

            for (int j = 0; j < GetMyCards.Count(); j++)
            {
                nb = new Button();
                nb.PreviewMouseLeftButtonDown += Button_MouseDown;
                nb.MouseEnter += Enter_Button;
                nb.MouseLeave += Leave_Button;
                HandGrid.Children.Add(nb);
                Grid.SetColumn(nb, j);
            }

            for (int j = 0; j < GetOpponentCardsCount; j++)
            {
                nb = new Button();
                Image image = new Image();
                string str = @"pack://application:,,,/images/Rubashka.jpg";
                image.Source = new BitmapImage(new Uri(str));
                nb.Content = image;
                OpponentHandGrid.Children.Add(nb);
                Grid.SetColumn(nb, j);
            }

            foreach (var button in HandGrid.Children.OfType<Button>())//Чистка карт в руке
            {
                button.Name = "";
                button.Content = "";
            }
            i = 0;
             
            foreach (var button in HandGrid.Children.OfType<Button>())//Обновление карт в руке
            {
                if (i > GetMyCards.Count() - 1) {
                    i = 0;
                    break;
                }
                button.Name = GetMyCards[i].Suit + "_" + GetMyCards[i].Value;
                string str = @"pack://application:,,,/Cards/" + GetMyCards[i].Suit + @"/" + GetMyCards[i].Value + ".jpg";
                Image image = new Image(); //чтобы присваивать изображения кнопкам
                image.Source = new BitmapImage(new Uri(str));
                button.Content = image;
                i++;
            }

        }
        #endregion

        #region Buttons clicks
        private void BitoButton_Click(object sender, RoutedEventArgs e)
        {
            client.MakeMove(RoomName, password, userID, null, null);
            RefreshButton_Click(sender, new RoutedEventArgs());
        }

        private void TakeButton_Click(object sender, RoutedEventArgs e)
        {
            client.MakeMove(RoomName, password, userID, null, null);
            RefreshButton_Click(sender, new RoutedEventArgs());
        }

        private void Enter_Button(object sender, RoutedEventArgs e)
        {
            Button enterButton = sender as Button;
            enterButton.Background = Brushes.Aquamarine;        
        }

        private void Leave_Button(object sender, RoutedEventArgs e)
        {
            Button enterButton = sender as Button;
            enterButton.Background = Brushes.ForestGreen;
        }

        private void Leave2_Button(object sender, RoutedEventArgs e)
        {
            Button enterButton = sender as Button;
            enterButton.Background = Brushes.LightGray;
        }
        #endregion

        protected override void OnClosing(CancelEventArgs e)
        {
            timer.Stop();
        }
    }
}
