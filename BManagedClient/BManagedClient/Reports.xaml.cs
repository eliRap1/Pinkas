using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BManagedClient
{
    public partial class Reports : Page
    {
        private bool _ready;

        public Reports()
        {
            InitializeComponent();
            if (!ClientSession.IsOwner) { NavigationService?.Navigate(new LogIn()); return; }
            for (int y = DateTime.Today.Year; y >= DateTime.Today.Year - 4; y--) yearCombo.Items.Add(y);
            yearCombo.SelectedIndex = 0;
            for (int m = 1; m <= 12; m++) monthCombo.Items.Add(m);
            monthCombo.SelectedIndex = DateTime.Today.Month - 1;
            _ready = true;
            Reload();
        }

        private void Filter_Changed(object s, SelectionChangedEventArgs e) { if (_ready) Reload(); }

        private void Reload()
        {
            try
            {
                int year  = (int)(yearCombo.SelectedItem ?? DateTime.Today.Year);
                int month = (int)(monthCombo.SelectedItem ?? DateTime.Today.Month);
                string cur = (curCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "ILS";

                var v = ServiceGateway.Use(c => c.GetVatSummary(LogIn.sign.Id, year, month, cur));
                if (v != null)
                {
                    vatColl.Text = v.VatCollected.ToString("N0") + " " + cur;
                    vatPaid.Text = v.VatPaid.ToString("N0") + " " + cur;
                    vatDue.Text  = v.VatDue.ToString("N0") + " " + cur;
                }
                topCustomers.ItemsSource = ServiceGateway.Use(c => c.GetTopCustomersByRevenue(LogIn.sign.Id, cur));
                var first = new DateTime(year, month, 1);
                var last  = first.AddMonths(1).AddDays(-1);
                breakdown.ItemsSource = ServiceGateway.Use(c => c.GetExpenseBreakdown(LogIn.sign.Id, first, last, cur));
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
        }

        private void Back_Click(object s, RoutedEventArgs e) => NavigationService?.Navigate(new OwnerHome());
    }
}
