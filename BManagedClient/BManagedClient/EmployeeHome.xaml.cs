using System;
using System.Windows;
using System.Windows.Controls;

namespace BManagedClient
{
    public partial class EmployeeHome : Page
    {
        public EmployeeHome()
        {
            InitializeComponent();
            welcome.Text = "Hello " + LogIn.sign.Username;
            if (!ClientSession.IsEmployee)
            {
                MessageBox.Show("Access denied.");
                NavigationService?.Navigate(new LogIn());
                return;
            }
            try
            {
                var projects = ServiceGateway.Use(c => c.GetProjectsForEmployee(LogIn.sign.Id));
                projectsList.ItemsSource = projects;
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
        }

        private void Logout_Click(object s, RoutedEventArgs e)
        {
            LogIn.sign = new Sign();
            NavigationService?.Navigate(new LogIn());
        }
    }
}
