using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BManagedClient
{
    public partial class Settings : Page
    {
        public Settings()
        {
            InitializeComponent();
            if (!ClientSession.IsLoggedIn) { NavigationService?.Navigate(new LogIn()); return; }
            emailBox.Text  = LogIn.sign.Email;
            phoneBox.Text  = LogIn.sign.Phone;
            curCombo.SelectedItem = curCombo.Items.OfType<ComboBoxItem>()
                .FirstOrDefault(x => x.Content?.ToString() == LogIn.sign.PreferredCurrency);
        }

        private void Save_Click(object s, RoutedEventArgs e)
        {
            try
            {
                string cur = (curCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "ILS";
                ServiceGateway.Use(c => c.UpdateUserProfile(LogIn.sign.Id, emailBox.Text, phoneBox.Text, cur));
                LogIn.sign.Email = emailBox.Text;
                LogIn.sign.Phone = phoneBox.Text;
                LogIn.sign.PreferredCurrency = cur;
                status.Text = "Saved.";
            }
            catch (Exception ex) { status.Text = "Save failed: " + ex.Message; }
        }

        private void UpdatePass_Click(object s, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(newPass.Password) || newPass.Password.Length < 4)
            { status.Text = "Password must be 4+ chars."; return; }
            try
            {
                ServiceGateway.Use(c => c.ResetPassword(LogIn.sign.Id, newPass.Password));
                newPass.Clear();
                status.Text = "Password updated.";
            }
            catch (Exception ex) { status.Text = "Failed: " + ex.Message; }
        }

        private void Back_Click(object s, RoutedEventArgs e)
            => NavigationService?.Navigate(ClientSession.IsOwner ? (Page)new OwnerHome()
                                       : ClientSession.IsEmployee ? new EmployeeHome()
                                       : new ClientHome());
    }
}
