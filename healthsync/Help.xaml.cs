using System.Windows;
using System.Windows.Controls;

namespace HealthSync
{
    public partial class HelpPage : Page
    {
        public HelpPage()
        {
            InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.ShowMainContent();
        }
    }
}