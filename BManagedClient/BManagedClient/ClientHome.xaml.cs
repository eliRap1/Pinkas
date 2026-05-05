using System.Windows;
using System.Windows.Controls;

namespace BManagedClient
{
    public partial class ClientHome : Page
    {
        public ClientHome()
        {
            InitializeComponent();
            welcome.Text = "Welcome, " + LogIn.sign.Username;
        }

        private void Logout_Click(object s, RoutedEventArgs e)
        {
            LogIn.sign = new Sign();
            NavigationService?.Navigate(new LogIn());
        }
    }
}
