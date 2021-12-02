using DurakApp.DurakServiceReference;
using System;
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

        private DispatcherTimer timer = new DispatcherTimer();



        private void timer_Tick(object sender, EventArgs e)
        {
            RefreshButton_Click(sender, new RoutedEventArgs());
        }

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

        private void Button_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Button clickButton = sender as Button; //эта строка преобразует переменную sender в метку с именем clickButton
            if (clickButton != null) {
                if (firstClicked == null) {
                    firstClicked = clickButton;
                    first = firstClicked.Name.ToString();
                    timer.Stop();
                    BitoButton.IsEnabled = false;
                    TakeButton.IsEnabled = false;
                    HandGrid.IsEnabled = false;
                    if (client.GetMoveOpportunity(RoomName, password, userID).CanMakeMove.ToString() == "CanNothing" || client.GetMoveOpportunity(RoomName, password, userID).CanMakeMove.ToString() == "CanDefend")
                    {
                        TableGrid.IsEnabled = true;
                        foreach(var button in TableGrid.Children.OfType<Button>())
                        {
                            if (button.Name == "")
                                button.IsEnabled = false;
                        }
                    }
                    if (client.GetMoveOpportunity(RoomName, password, userID).CanMakeMove.ToString() == "CanAttack" || client.GetMoveOpportunity(RoomName, password, userID).CanMakeMove.ToString() == "CanThrow" || client.GetMoveOpportunity(RoomName, password, userID).CanMakeMove.ToString() == "CanThrowAfter")
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
                        foreach (var card in client.GetMyCards(RoomName, password, userID)) {
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
                        foreach (var list in client.GetCardsOnTable(RoomName, password, userID)) {
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
                        client.MakeMove(RoomName, password, userID, client.GetMyCards(RoomName, password, userID)[i1], null);
                    else
                        client.MakeMove(RoomName, password, userID, client.GetMyCards(RoomName, password, userID)[i1], client.GetCardsOnTable(RoomName, password, userID)[b1][b2]);
                    first = "";
                    firstClicked = null;
                    second = "";
                    secondClicked = null;
                    HandGrid.IsEnabled = true;
                    if (client.GetMoveOpportunity(RoomName, password, userID).CanMakeMove.ToString() == "YouWin" || client.GetMoveOpportunity(RoomName, password, userID).CanMakeMove.ToString() == "YouLose")
                    {
                        client.Close();
                        timer.Stop();
                        return;
                    }
                    RefreshButton_Click(sender, new RoutedEventArgs());
                    timer.Start();
                    return;

                }
            }//проверяет произошло ли успешное преобразование из строки выше, если не null то всё ок, если null то это бы означало что мы нажали не на кнопку, а на чето другое
        }//обрабатывает событие клика
        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (client.GetMoveOpportunity(RoomName, password, userID).CanMakeMove.ToString() == "YouWin" || client.GetMoveOpportunity(RoomName, password, userID).CanMakeMove.ToString() == "YouLose")
            {
                MessageBox.Show(client.GetMoveOpportunity(RoomName, password, userID).CanMakeMove.ToString());
                client.Close();
            }

            TableGrid.ColumnDefinitions.Clear();
            TableGrid2.ColumnDefinitions.Clear();
            TableGrid.Children.Clear();
            TableGrid2.Children.Clear();
            ColumnDefinition cd;
            for (int j = 0; j < client.GetCardsOnTable(RoomName, password, userID).Count() + 1; j++)
            {
                cd = new ColumnDefinition();
                TableGrid.ColumnDefinitions.Add(cd);
            }

            for (int j = 0; j < client.GetCardsOnTable(RoomName, password, userID).Count() + 1; j++)
            {
                cd = new ColumnDefinition();
                TableGrid2.ColumnDefinitions.Add(cd);
            }

            Button nb;
            for (int j = 0; j < client.GetCardsOnTable(RoomName, password, userID).Count() + 1; j++)
            {
                nb = new Button();
                nb.PreviewMouseLeftButtonDown += Button_MouseDown;
                nb.MouseEnter += Enter_Button;
                nb.MouseLeave += Leave_Button;
                TableGrid.Children.Add(nb);
                Grid.SetColumn(nb, j);
            }

            for (int j = 0; j < client.GetCardsOnTable(RoomName, password, userID).Count() + 1; j++)
            {
                nb = new Button();
                nb.PreviewMouseLeftButtonDown += Button_MouseDown;
                nb.MouseEnter += Enter_Button;
                nb.MouseLeave += Leave_Button;
                TableGrid2.Children.Add(nb);
                Grid.SetColumn(nb, j);
            }


            int i = 0;
            OponentCardsCount.Content = client.GetOpponentCardsCount(RoomName, password, userID);
            StockButton.Content = client.GetCardsInStockCount(RoomName, password, userID);
            TestButton.Content = client.GetMoveOpportunity(RoomName, password, userID).CanMakeMove.ToString();           
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

            if (client.GetMoveOpportunity(RoomName, password, userID).CanMakeMove.ToString() == "CanNothing" || client.GetMoveOpportunity(RoomName, password, userID).CanMakeMove.ToString() == "CanDefend") {

                BitoButton.Visibility = Visibility.Hidden;
                TakeButton.Visibility = Visibility.Visible;
                TableGrid2.IsEnabled = false;
                TableGrid.IsEnabled = false;
                bool b = true;
                foreach (var list in client.GetCardsOnTable(RoomName, password, userID))
                    if (list.Count() != 2)
                    {
                        b = false;
                        break;
                    }
                if (b || client.GetMoveOpportunity(RoomName, password, userID).CanMakeMove.ToString() == "CanNothing")
                    TakeButton.IsEnabled = false;
                else
                    TakeButton.IsEnabled = true;

                foreach (var button in TableGrid.Children.OfType<Button>()) {
                    if (i > client.GetCardsOnTable(RoomName, password, userID).Count() - 1) {
                        i = 0;
                        break;
                    }
                    button.Name = client.GetCardsOnTable(RoomName, password, userID)[i][0].Suit + "_" + client.GetCardsOnTable(RoomName, password, userID)[i][0].Value;
                    string str = @"pack://application:,,,/Cards/" + client.GetCardsOnTable(RoomName, password, userID)[i][0].Suit + @"/" + client.GetCardsOnTable(RoomName, password, userID)[i][0].Value + ".jpg";
                    Image image = new Image(); //чтобы присваивать изображения кнопкам
                    image.Source = new BitmapImage(new Uri(str));
                    button.Content = image; 
                    i++;
                }
                foreach (var button in TableGrid2.Children.OfType<Button>()) {
                    if (i > client.GetCardsOnTable(RoomName, password, userID).Count() - 1){
                        i = 0;
                        break;
                    }
                    if (client.GetCardsOnTable(RoomName, password, userID)[i].Count() < 2)
                    {
                        i++;
                        continue;
                    }
                    button.Name = client.GetCardsOnTable(RoomName, password, userID)[i][1].Suit + "_" + client.GetCardsOnTable(RoomName, password, userID)[i][1].Value;
                    string str = @"pack://application:,,,/Cards/" + client.GetCardsOnTable(RoomName, password, userID)[i][1].Suit + @"/" + client.GetCardsOnTable(RoomName, password, userID)[i][1].Value + ".jpg";
                    Image image = new Image(); //чтобы присваивать изображения кнопкам
                    image.Source = new BitmapImage(new Uri(str));
                    button.Content = image;
                    i++;
                }
            }
            if (client.GetMoveOpportunity(RoomName, password, userID).CanMakeMove.ToString() == "CanAttack" || client.GetMoveOpportunity(RoomName, password, userID).CanMakeMove.ToString() == "CanThrow" || client.GetMoveOpportunity(RoomName, password, userID).CanMakeMove.ToString() == "CanThrowAfter") {
                TakeButton.Visibility = Visibility.Hidden;
                BitoButton.Visibility = Visibility.Visible;
                TableGrid.IsEnabled = false;
                TableGrid2.IsEnabled = false;
                bool b = true;
                foreach (var list in client.GetCardsOnTable(RoomName, password, userID))
                    if (list.Count() < 2)
                    {
                        b = false;
                        break;
                    }
                if ((!b || client.GetMoveOpportunity(RoomName, password, userID).CanMakeMove.ToString() == "CanAttack") && !(client.GetMoveOpportunity(RoomName, password, userID).CanMakeMove.ToString() == "CanThrowAfter"))
                    BitoButton.IsEnabled = false;
                else
                    BitoButton.IsEnabled = true;             
                foreach (var button in TableGrid2.Children.OfType<Button>()) {
                    if (i > client.GetCardsOnTable(RoomName, password, userID).Count() - 1) {
                        i = 0;
                        break;
                    }
                    button.Name = client.GetCardsOnTable(RoomName, password, userID)[i][0].Suit + "_" + client.GetCardsOnTable(RoomName, password, userID)[i][0].Value;
                    string str = @"pack://application:,,,/Cards/" + client.GetCardsOnTable(RoomName, password, userID)[i][0].Suit + @"/" + client.GetCardsOnTable(RoomName, password, userID)[i][0].Value + ".jpg";
                    Image image = new Image(); //чтобы присваивать изображения кнопкам
                    image.Source = new BitmapImage(new Uri(str));
                    button.Content = image;
                    i++;
                }
                foreach (var button in TableGrid.Children.OfType<Button>()) {
                    if (i > client.GetCardsOnTable(RoomName, password, userID).Count() - 1 ) {
                        i = 0;
                        break;
                    }
                    if (client.GetCardsOnTable(RoomName, password, userID)[i].Count() < 2)
                    {
                        i++;
                        continue;
                    }
                    button.Name = client.GetCardsOnTable(RoomName, password, userID)[i][1].Suit + "_" + client.GetCardsOnTable(RoomName, password, userID)[i][1].Value;
                    string str = @"pack://application:,,,/Cards/" + client.GetCardsOnTable(RoomName, password, userID)[i][1].Suit + @"/" + client.GetCardsOnTable(RoomName, password, userID)[i][1].Value + ".jpg";
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
            for (int j = 0; j < client.GetMyCards(RoomName, password, userID).Count(); j++)
            {
                cd = new ColumnDefinition();
                HandGrid.ColumnDefinitions.Add(cd);
            }

            for (int j = 0; j < client.GetOpponentCardsCount(RoomName, password, userID); j++)
            {
                cd = new ColumnDefinition();
                OpponentHandGrid.ColumnDefinitions.Add(cd);
            }

            for (int j = 0; j < client.GetMyCards(RoomName, password, userID).Count(); j++)
            {
                nb = new Button();
                nb.PreviewMouseLeftButtonDown += Button_MouseDown;
                nb.MouseEnter += Enter_Button;
                nb.MouseLeave += Leave_Button;
                HandGrid.Children.Add(nb);
                Grid.SetColumn(nb, j);
            }

            for (int j = 0; j < client.GetOpponentCardsCount(RoomName, password, userID); j++)
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
                if (i > client.GetMyCards(RoomName, password, userID).Count() - 1) {
                    i = 0;
                    break;
                }
                button.Name = client.GetMyCards(RoomName, password, userID)[i].Suit + "_" + client.GetMyCards(RoomName, password, userID)[i].Value;
                string str = @"pack://application:,,,/Cards/" + client.GetMyCards(RoomName, password, userID)[i].Suit + @"/" + client.GetMyCards(RoomName, password, userID)[i].Value + ".jpg";
                Image image = new Image(); //чтобы присваивать изображения кнопкам
                image.Source = new BitmapImage(new Uri(str));
                button.Content = image;
                i++;
            }

        }

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
    }
}
