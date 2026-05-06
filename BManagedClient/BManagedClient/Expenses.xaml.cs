using BManagedClient.BMsrv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BManagedClient
{
    public partial class Expenses : Page
    {
        public Expenses()
        {
            InitializeComponent();
            if (!ClientSession.IsOwner && !ClientSession.IsEmployee)
            { NavigationService?.Navigate(new LogIn()); return; }
            LoadCategories();
            Refresh();
        }

        private void LoadCategories()
        {
            var arr = ServiceGateway.Use(c => c.GetExpenseCategories());
            catCombo.ItemsSource = arr;
            if (arr != null && arr.Length > 0) catCombo.SelectedIndex = 0;
        }

        private void Refresh()
        {
            var arr = ServiceGateway.Use(c => c.GetExpensesByOwner(LogIn.sign.Id));
            expList.ItemsSource = arr;
        }

        private void Add_Click(object s, RoutedEventArgs e)
        {
            decimal.TryParse(amountBox.Text, out decimal amt);
            decimal.TryParse(vatBox.Text, out decimal vat);
            try
            {
                ServiceGateway.Use(c => c.AddExpense(new Expense
                {
                    OwnerId = LogIn.sign.Id,
                    CategoryId = catCombo.SelectedValue is int cid ? cid : (int?)null,
                    Date = DateTime.Today,
                    Amount = amt,
                    VatPaid = vat,
                    Vendor = vendorBox.Text ?? "",
                    Currency = LogIn.sign.PreferredCurrency
                }));
                vendorBox.Text = ""; amountBox.Text = "0"; vatBox.Text = "0";
                Refresh();
            }
            catch (Exception ex) { MessageBox.Show("Add failed: " + ex.Message); }
        }

        private void Back_Click(object s, RoutedEventArgs e)
            => NavigationService?.Navigate(ClientSession.IsOwner ? (Page)new OwnerHome() : new EmployeeHome());

        // Auto VAT: from a gross amount, derive the VAT portion at 17 % (Israeli standard).
        // gross = net + vat = net + net*0.17 = net*1.17 → vat = gross * 17 / 117.
        private void AutoVat_Click(object s, RoutedEventArgs e)
        {
            if (decimal.TryParse(amountBox.Text, out decimal gross) && gross > 0)
            {
                decimal vat = Math.Round(gross * 17m / 117m, 2);
                vatBox.Text = vat.ToString("0.##");
            }
        }

        // Live recalc: when the user types in Amount, refresh VAT only if they
        // haven't manually overridden it yet (VAT still 0 / blank).
        private void Amount_Changed(object s, TextChangedEventArgs e)
        {
            if (vatBox == null) return;
            if (string.IsNullOrWhiteSpace(vatBox.Text) || vatBox.Text.Trim() == "0")
            {
                if (decimal.TryParse(amountBox.Text, out decimal gross) && gross > 0)
                    vatBox.Text = Math.Round(gross * 17m / 117m, 2).ToString("0.##");
            }
        }
    }
}
