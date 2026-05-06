using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace BManagedClient
{
    public partial class Notifications : Page
    {
        private DispatcherTimer _pollTimer;

        public Notifications()
        {
            InitializeComponent();
            if (!ClientSession.IsLoggedIn) { NavigationService?.Navigate(new LogIn()); return; }
            Refresh();
            _pollTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(10) };
            _pollTimer.Tick += (s, e) => Refresh();
            _pollTimer.Start();
            Unloaded += (s, e) => _pollTimer.Stop();
        }

        private void Refresh()
        {
            try
            {
                var arr = ServiceGateway.Use(c => c.GetUserNotifications(LogIn.sign.Id));
                notifList.ItemsSource = arr;
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
        }

        private void MarkAll_Click(object s, RoutedEventArgs e)
        {
            try
            {
                ServiceGateway.Use(c => c.MarkAllNotificationsAsRead(LogIn.sign.Id));
                Refresh();
            }
            catch (Exception ex) { MessageBox.Show("Failed: " + ex.Message); }
        }

        private void Back_Click(object s, RoutedEventArgs e)
        {
            _pollTimer?.Stop();
            NavigationService?.Navigate(ClientSession.IsOwner ? (Page)new OwnerHome()
                                       : ClientSession.IsEmployee ? new EmployeeHome()
                                       : new ClientHome());
        }
    }
}
