using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace BManagedClient
{
    /// <summary>
    /// Owner dashboard. Polls server every 15s to refresh stats and the
    /// notification badge — same DispatcherTimer pattern as Driver-moodle.
    /// </summary>
    public partial class OwnerHome : Page
    {
        private DispatcherTimer pollTimer;

        public OwnerHome()
        {
            InitializeComponent();
            welcome.Text = LogIn.sign.Username;

            // Guard wrong role.
            if (!ClientSession.IsOwner)
            {
                MessageBox.Show("Access denied.");
                NavigationService?.Navigate(new LogIn());
                return;
            }

            RefreshStats();

            pollTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(15) };
            pollTimer.Tick += (s, e) => RefreshStats();
            pollTimer.Start();
            Unloaded += (s, e) => pollTimer.Stop();
        }

        private void RefreshStats()
        {
            try
            {
                var customers = ServiceGateway.Use(c => c.GetCustomersForOwner(LogIn.sign.Id));
                customersCount.Text = (customers?.Length ?? 0).ToString();

                var unpaid = ServiceGateway.Use(c => c.GetUnpaidInvoices(LogIn.sign.Id));
                unpaidCount.Text = (unpaid?.Length ?? 0).ToString();

                var overdue = ServiceGateway.Use(c => c.GetOverdueInvoices(LogIn.sign.Id));
                overdueCount.Text = (overdue?.Length ?? 0).ToString();

                var projs = ServiceGateway.Use(c => c.GetProjectsByStatus("Active", LogIn.sign.Id));
                activeProjects.Text = (projs?.Length ?? 0).ToString();
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("RefreshStats: " + ex.Message); }
        }

        private void Customers_Click(object s, RoutedEventArgs e)     => page.Navigate(new Customers());
        private void Projects_Click(object s, RoutedEventArgs e)      => MessageBox.Show("Projects screen — TODO");
        private void Invoices_Click(object s, RoutedEventArgs e)      => MessageBox.Show("Invoices screen — TODO");
        private void Expenses_Click(object s, RoutedEventArgs e)      => MessageBox.Show("Expenses screen — TODO");
        private void Reports_Click(object s, RoutedEventArgs e)       => MessageBox.Show("Reports screen — TODO");
        private void Settings_Click(object s, RoutedEventArgs e)      => MessageBox.Show("Settings screen — TODO");
        private void Notifications_Click(object s, RoutedEventArgs e) => MessageBox.Show("Notifications — TODO");

        private void Logout_Click(object s, RoutedEventArgs e)
        {
            if (MessageBox.Show("Logout?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                pollTimer?.Stop();
                LogIn.sign = new Sign();
                NavigationService?.Navigate(new LogIn());
            }
        }
    }
}
