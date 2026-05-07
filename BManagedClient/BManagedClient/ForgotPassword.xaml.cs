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
                // Resolve user, find their company's Owner, and notify only
                // that one Owner. Avoids leaking the request to every Owner on
                // the server (which is what GetAllUsers() previously did).
                ServiceGateway.Use(c =>
                {
                    if (!c.CheckUserExist(u)) { Show("User not found.", false); return; }
                    int uid = c.GetUserId(u);
                    var user = c.GetUserById(uid);
                    if (user == null) { Show("User not found.", false); return; }

                    int? ownerId = user.Role == "Owner" ? (int?)user.Id : user.OwnerId;
                    if (!ownerId.HasValue || ownerId.Value <= 0)
                    { Show("No company Owner is linked to this account. Ask an admin.", false); return; }

                    c.SendNotification(new Notification
                    {
                        UserId           = ownerId.Value,
                        Title            = "Password reset request",
                        Message          = "User '" + user.Username + "' (" + user.Role +
                                           ") asked for a password reset. Open Manage Users → Reset PW.",
                        NotificationType = "ResetRequest",
                        IsRead           = false,
                        CreatedAt        = DateTime.Now,
                    });
                    Show("Your company's Owner has been notified.", true);
                });
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
