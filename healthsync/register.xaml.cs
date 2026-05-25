using System;
using System.Windows;
using System.Windows.Controls;

namespace HealthSync
{
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameBox.Text.Trim();
            string email = EmailBox.Text.Trim();
            string password = PasswordBox1.Password;
            string confirmPassword = PasswordBox2.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Заполните все поля!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Пароли не совпадают!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(AgeBox.Text, out int age) || age < 10 || age > 120)
            {
                MessageBox.Show("Введите корректный возраст (10-120)!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!double.TryParse(HeightBox.Text, out double height) || height < 100 || height > 250)
            {
                MessageBox.Show("Введите корректный рост (100-250 см)!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!double.TryParse(WeightBox.Text, out double weight) || weight < 20 || weight > 300)
            {
                MessageBox.Show("Введите корректный вес (20-300 кг)!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string gender = (GenderBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            var (success, error) = await MainWindow.Api.Register(username, email, password, height, weight, age, gender);

            if (success)
            {
                MessageBox.Show("Регистрация успешна! Теперь вы можете войти.", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                MainWindow.Instance.NavigateToPage(new LoginPage());
            }
            else
            {
                MessageBox.Show($"Ошибка регистрации: {error}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.NavigateToPage(new LoginPage());
        }
    }
}