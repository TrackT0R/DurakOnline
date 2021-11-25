using DurakApp.DurakServiceReference;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        #endregion

        string first = ""; // эта штука понадобится, объяснения будут ниже
        string second = ""; // тоже понадобится
        Button firstClicked = null; // Это ссылочная переменная!!!!!!!!! Она не добавляет новую кнопку в wpf, т.к не использует ключевое слово new и объект тем самым не создается
        Button secondClicked = null; // Тоже ссылочная переменная. Они будут ссылаться на наши кнопки, на которые мы кликаем

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
                    var s = client.GetMyCards(RoomName, password, userID);
                    if (s2.Length < 2)
                        client.MakeMove(RoomName, password, userID, client.GetMyCards(RoomName, password, userID)[i1], null);
                    else
                        client.MakeMove(RoomName, password, userID, client.GetMyCards(RoomName, password, userID)[i1], client.GetCardsOnTable(RoomName, password, userID)[b1][b2]);
                    first = "";
                    firstClicked = null;
                    second = "";
                    secondClicked = null;
                    return;

                }
            }//проверяет произошло ли успешное преобразование из строки выше, если не null то всё ок, если null то это бы означало что мы нажали не на кнопку, а на чето другое
        }//обрабатывает событие клика
        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            int i = 0;
            OponentCardsCount.Content = client.GetOpponentCardsCount(RoomName, password, userID);
            StockButton.Content = client.GetCardsInStockCount(RoomName, password, userID);
            TestButton.Content = client.GetMoveOpportunity(RoomName, password, userID).CanMakeMove.ToString();
            foreach (var button in TableGrid.Children.OfType<Button>())//Чистка карт на столе
            {
                button.Name = "";
                Image image = new Image(); //чтобы присваивать изображения кнопкам
                image.Source = new BitmapImage(new Uri(@"pack://application:,,,/images/white.jpg"));
                button.Content = image;
            }
            foreach (var button in TableGrid2.Children.OfType<Button>())//Чистка карт на столе
            {
                button.Name = "";
                Image image = new Image(); //чтобы присваивать изображения кнопкам
                image.Source = new BitmapImage(new Uri(@"pack://application:,,,/images/white.jpg"));
                button.Content = image;
            }

            if (client.GetMoveOpportunity(RoomName, password, userID).CanMakeMove.ToString() == "CanNothing" || client.GetMoveOpportunity(RoomName, password, userID).CanMakeMove.ToString() == "CanDefend") {
                BitoButton.IsEnabled = false;
                TableGrid2.IsEnabled = false;
                TableGrid.IsEnabled = true;
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
                    if (i > client.GetCardsOnTable(RoomName, password, userID).Count() - 1 || client.GetCardsOnTable(RoomName, password, userID)[i].Count() < 2) {
                        i = 0;
                        break;
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
                TakeButton.IsEnabled = false;
                TableGrid.IsEnabled = false;
                BitoButton.IsEnabled = true;
                TableGrid2.IsEnabled = true;
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
                    if (i > client.GetCardsOnTable(RoomName, password, userID).Count() - 1 || client.GetCardsOnTable(RoomName, password, userID)[i].Count() < 2) {
                        i = 0;
                        break;
                    }
                    button.Name = client.GetCardsOnTable(RoomName, password, userID)[i][1].Suit + "_" + client.GetCardsOnTable(RoomName, password, userID)[i][1].Value;
                    string str = @"pack://application:,,,/Cards/" + client.GetCardsOnTable(RoomName, password, userID)[i][1].Suit + @"/" + client.GetCardsOnTable(RoomName, password, userID)[i][1].Value + ".jpg";
                    Image image = new Image(); //чтобы присваивать изображения кнопкам
                    image.Source = new BitmapImage(new Uri(str));
                    button.Content = image;
                    i++;
                }
            }
            foreach (var button in HandGrid.Children.OfType<Button>())//Чистка карт в руке
            {
                button.Name = "";
                button.Content = "";
            }
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
        }

        private void TakeButton_Click(object sender, RoutedEventArgs e)
        {
            client.MakeMove(RoomName, password, userID, null, null);
        }
    }
}
