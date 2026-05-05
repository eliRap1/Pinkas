using System.Windows;

namespace BManagedClient
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            page.Navigate(new LogIn());
        }
    }
}
