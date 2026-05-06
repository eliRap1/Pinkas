using BManagedClient.BMsrv;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace BManagedClient
{
    public partial class SignUp : Page
    {
        private static readonly Regex EmailRx = new Regex(@"^[\w\.\-]+@[\w\-]+\.[\w\-\.]+$");
        private static readonly Regex PhoneRx = new Regex(@"^\+?\d{7,15}$");

        // "Owner" or "Employee" — set when the user picks a path. Empty = step 1 still.
        private string _selectedRole = "";

        public SignUp() => InitializeComponent();

        private void ChooseOwner_Click(object s, RoutedEventArgs e)
            => SwitchToForm("Owner");

        private void ChooseEmployee_Click(object s, RoutedEventArgs e)
            => SwitchToForm("Employee");

        private void SwitchToForm(string role)
        {
            _selectedRole = role;
            roleStep.Visibility = Visibility.Collapsed;
            formStep.Visibility = Visibility.Visible;
            ownerExtras.Visibility = role == "Owner" ? Visibility.Visible : Visibility.Collapsed;
            titleText.Text = role == "Owner" ? "Owner sign-up" : "Employee sign-up";
            subtitleText.Text = role == "Owner"
                ? "Run the business — you'll log invoices, expenses, and VAT."
                : "Work on assigned projects and log your expenses.";
        }

        private void ResetSteps_Click(object s, RoutedEventArgs e)
        {
            _selectedRole = "";
            formStep.Visibility = Visibility.Collapsed;
            roleStep.Visibility = Visibility.Visible;
            titleText.Text = "Create account";
            subtitleText.Text = "Pick the role that fits you.";
            status.Text = "";
        }

        private void Biz_Changed(object s, SelectionChangedEventArgs e)
        {
            // Zair is allowed for either Patur or Murshe — leave checkbox enabled.
        }

        private void Create_Click(object s, RoutedEventArgs e)
        {
            if (_selectedRole != "Owner" && _selectedRole != "Employee")
            { ResetSteps_Click(s, e); return; }

            string u = usernameBox.Text?.Trim() ?? "";
            string p = passwordBox.Password ?? "";
            string em = emailBox.Text?.Trim() ?? "";
            string ph = phoneBox.Text?.Trim() ?? "";
            string cur = (curBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "ILS";

            if (u.Length < 4 || p.Length < 4) { status.Text = "Username + password must be 4+ chars."; return; }
            if (!EmailRx.IsMatch(em)) { status.Text = "Email looks invalid."; return; }
            if (!PhoneRx.IsMatch(ph)) { status.Text = "Phone looks invalid."; return; }

            string bizType = "Individual";
            bool isZair = false;
            if (_selectedRole == "Owner")
            {
                bizType = (bizBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Patur";
                isZair = zairBox.IsChecked == true;
            }

            try
            {
                bool exists = ServiceGateway.Use(c => c.CheckUserExist(u));
                if (exists) { status.Text = "Username already taken."; return; }

                bool ok = ServiceGateway.Use(c => c.AddUser(u, p, em, ph, _selectedRole, cur));
                if (!ok) { status.Text = "Server rejected the request."; return; }

                if (_selectedRole == "Owner")
                {
                    try
                    {
                        int newId = ServiceGateway.Use(c => c.GetUserId(u));
                        ServiceGateway.Use(c => c.SetBusinessType(newId, bizType));
                        if (isZair) ServiceGateway.Use(c => c.SetIsZair(newId, true));
                    }
                    catch { /* business-type set is best-effort; account exists either way */ }
                }

                string msg = _selectedRole == "Owner"
                    ? "Owner account created. You can sign in now."
                    : "Employee account created. An existing Owner must approve the account before you can sign in.";
                MessageBox.Show(msg, "Welcome", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService?.Navigate(new LogIn());
            }
            catch (Exception ex) { status.Text = "Error: " + ex.Message; }
        }

        private void Back_Click(object s, RoutedEventArgs e)
            => NavigationService?.Navigate(new LogIn());
    }
}
