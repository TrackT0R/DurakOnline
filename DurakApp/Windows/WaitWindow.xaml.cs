using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using DurakApp.DurakServiceReference;

namespace DurakApp.Windows
{
    /// <summary>
    /// Логика взаимодействия для WaitWindow.xaml
    /// </summary>
    public partial class WaitWindow : Window
    {
        #region Data
        private DurakServiceClient client;
        private string RoomName;
        private DispatcherTimer timer = new DispatcherTimer();
        #endregion

        #region Initializaion
        public WaitWindow(DurakServiceClient client, string RoomName)
        {
            this.client = client;
            this.RoomName = RoomName;
            InitializeComponent();

            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 200);
            timer.Start();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (label.Content.ToString() == "...")
                label.Content = "";
            else
                label.Content += ".";
            if (!client.GetFreeRooms().Any(x => x == RoomName))
            {
                timer.Stop();
                DialogResult = true;
            }
        }
        #endregion

        #region Service methods
        protected override void OnClosing(CancelEventArgs e)
        {
            timer.Stop();
        }
        #endregion
    }
}
