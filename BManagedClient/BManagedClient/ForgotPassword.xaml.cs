using BManagedClient.BMsrv;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BManagedClient
{
    public partial class ForgotPassword : Page
    {
        public ForgotPassword() => InitializeComponent();

        private void Request_Click(object sender, RoutedEventArgs e)
        {
            string u = usernameBox.Text?.Trim() ?? "";
            if (u.Length < 4) { Show("Enter a valid username.", false); return; }

            try
            {
                bool exists = ServiceGateway.Use(c => c.CheckUserExist(u));
                if (!exists) { Show("User not found.", false); return; }

                int uid = ServiceGateway.Use(c => c.GetUserId(u));
                var user = ServiceGateway.Use(c => c.GetUserById(uid));
                var users = ServiceGateway.Use(c => c.GetAllUsers());

                if (users != null)
                {
                    foreach (var owner in users.Where(x => x.Role == "Owner" && x.IsActive))
                    {
                        ServiceGateway.Use(c => c.SendNotification(new Notification
                        {
                            UserId           = owner.Id,
                            Title            = "Password reset request",
                            Message          = "User '" + user.Username + "' (" + user.Role +
                                               ") asked for a password reset. Open Manage Users → Reset PW.",
                            NotificationType = "ResetRequest",
                            IsRead           = false,
                            CreatedAt        = DateTime.Now,
                        }));
                    }
                }
                Show("Owner notified. Wait for them to reset your password.", true);
            }
            catch (Exception ex) { Show("Error: " + ex.Message, false); }
        }

        private void Show(string msg, bool ok)
        {
            status.Text = msg;
            status.Foreground = ok
                ? (Brush)Application.Current.Resources["Mint"]
                : (Brush)Application.Current.Resources["Rose"];
        }

        private void Back_Click(object s, RoutedEventArgs e)
            => NavigationService?.Navigate(new LogIn());
    }
}
