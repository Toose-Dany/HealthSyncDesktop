using System.Windows;
using System.Windows.Controls;

namespace HealthSync
{
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            InitializeComponent();
            LoadUserData();
        }

        private void LoadUserData()
        {
            var user = MainWindow.Instance.CurrentUser;
            if (user != null)
            {
                UsernameBox.Text = user.Username;
                EmailBox.Text = user.Email;
                AgeBox.Text = user.Age.ToString();
                HeightBox.Text = user.Height.ToString();
                WeightBox.Text = user.Weight.ToString();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var user = MainWindow.Instance.CurrentUser;
            if (user != null)
            {
                user.Username = UsernameBox.Text;
                user.Email = EmailBox.Text;

                if (int.TryParse(AgeBox.Text, out int age))
                    user.Age = age;

                if (double.TryParse(HeightBox.Text, out double height))
                    user.Height = height;

                if (double.TryParse(WeightBox.Text, out double weight))
                    user.Weight = weight;

                MainWindow.Instance.ShowNotification("Профиль обновлен!");
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.ShowMainContent();
        }
    }
}