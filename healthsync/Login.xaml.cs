using System;
using System.Windows;
using System.Windows.Controls;

namespace HealthSync
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var user = UserManager.LoginUser(username, password);

            if (user != null)
            {
                MainWindow.Instance.CurrentUser = user;
                MainWindow.Instance.LoadUserData();
                MainWindow.Instance.ShowMainContent();
                MessageBox.Show($"Добро пожаловать, {user.Username}!", "Успешно",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль!\n\nИспользуйте: Пользователь / 123", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.NavigateToPage(new RegisterPage());
        }
    }
}