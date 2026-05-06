using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

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

        private void ExportCsv_Click(object s, RoutedEventArgs e)
        {
            try
            {
                int year  = (int)(yearCombo.SelectedItem ?? DateTime.Today.Year);
                int month = (int)(monthCombo.SelectedItem ?? DateTime.Today.Month);
                string cur = (curCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "ILS";

                var v   = ServiceGateway.Use(c => c.GetVatSummary(LogIn.sign.Id, year, month, cur));
                var top = ServiceGateway.Use(c => c.GetTopCustomersByRevenue(LogIn.sign.Id, cur));
                var first = new DateTime(year, month, 1);
                var last  = first.AddMonths(1).AddDays(-1);
                var brk = ServiceGateway.Use(c => c.GetExpenseBreakdown(LogIn.sign.Id, first, last, cur));

                var sb = new StringBuilder();
                sb.AppendLine("B-Managed report — " + year + "-" + month.ToString("D2") + " (" + cur + ")");
                sb.AppendLine();
                sb.AppendLine("VAT collected,VAT paid,VAT due,Income,Expenses");
                if (v != null)
                {
                    sb.AppendLine(string.Join(",",
                        v.VatCollected, v.VatPaid, v.VatDue, v.IncomeTotal, v.ExpensesTotal));
                }
                sb.AppendLine();
                sb.AppendLine("Top customers");
                sb.AppendLine("Customer,Invoiced,Paid");
                if (top != null)
                    foreach (var r in top)
                        sb.AppendLine(Csv(r.BusinessName) + "," + r.TotalInvoiced + "," + r.TotalPaid);
                sb.AppendLine();
                sb.AppendLine("Expense breakdown");
                sb.AppendLine("Category,Total,VAT-deductible");
                if (brk != null)
                    foreach (var r in brk)
                        sb.AppendLine(Csv(r.CategoryName) + "," + r.Total + "," + r.IsVatDeductible);

                var dlg = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv",
                    FileName = "BManaged_Report_" + year + "_" + month.ToString("D2") + ".csv"
                };
                if (dlg.ShowDialog() == true)
                {
                    File.WriteAllText(dlg.FileName, sb.ToString(), new UTF8Encoding(true));
                    MessageBox.Show("Saved.\n" + dlg.FileName, "Export", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Export failed: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string Csv(string s)
        {
            if (s == null) return "";
            if (s.Contains(",") || s.Contains("\"") || s.Contains("\n"))
                return "\"" + s.Replace("\"", "\"\"") + "\"";
            return s;
        }
    }
}
