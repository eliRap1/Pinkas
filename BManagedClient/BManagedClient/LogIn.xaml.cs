using BManagedClient.BMsrv;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BManagedClient
{
    public partial class LogIn : Page
    {
        public static Sign sign = new Sign();

        public LogIn()
        {
            InitializeComponent();
            DataContext = sign;
        }

        private async void signIn_Click(object sender, RoutedEventArgs e)
        {
            error.Visibility = Visibility.Collapsed;
            string u = username.Text?.Trim() ?? "";
            string p = pass.Password ?? "";
            if (string.IsNullOrEmpty(u) || string.IsNullOrEmpty(p))
            {
                ShowError("Fill both fields.");
                return;
            }

            // Disable inputs + show busy cursor so the user knows we're working
            // and so the UI thread isn't blocked by the synchronous WCF call
            // (the cause of the previous "Not Responding" freeze on slow
            // first-channel-open).
            var btn = sender as Button;
            if (btn != null) btn.IsEnabled = false;
            username.IsEnabled = false;
            pass.IsEnabled = false;
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                User user = null;
                try
                {
                    user = await Task.Run(() => ServiceGateway.Use(c =>
                        c.CheckUserPassword(u, p) ? c.GetUserById(c.GetUserId(u)) : null));
                }
                catch (Exception ex)
                {
                    ShowError("Connection error: " + ex.Message);
                    return;
                }
                if (user == null) { ShowError("Wrong username or password."); return; }

                sign.Username          = user.Username;
                sign.Password          = p;
                sign.Email             = user.Email;
                sign.Phone             = user.Phone;
                sign.Id                = user.Id;
                sign.Role              = user.Role;
                sign.PreferredCurrency = user.PreferredCurrency;
                sign.IsActive          = user.IsActive;
                sign.BusinessType      = string.IsNullOrEmpty(user.BusinessType) ? "Individual" : user.BusinessType;
                sign.IsZair            = user.IsZair;

                bool tempPassword = string.Equals(p, "reset1234", StringComparison.Ordinal);
                if (tempPassword)
                {
                    MessageBox.Show(
                        "You're signed in with the temporary password 'reset1234'.\n" +
                        "Please choose a new password in the Settings page.",
                        "Change your password",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }

                page.Visibility = Visibility.Visible;
                if (tempPassword)         page.Navigate(new Settings());
                else if (sign.IsOwner)    page.Navigate(new OwnerHome());
                else if (sign.IsEmployee) page.Navigate(new EmployeeHome());
                else                      page.Navigate(new ClientHome());
            }
            finally
            {
                Mouse.OverrideCursor = null;
                if (btn != null) btn.IsEnabled = true;
                username.IsEnabled = true;
                pass.IsEnabled = true;
            }
        }

        private void ShowError(string msg)
        {
            error.Text = msg;
            error.Visibility = Visibility.Visible;
        }

        private void signUp_Click(object sender, RoutedEventArgs e)
            => NavigationService?.Navigate(new SignUp());

        private void forgot_Click(object sender, RoutedEventArgs e)
            => NavigationService?.Navigate(new ForgotPassword());
    }
}
