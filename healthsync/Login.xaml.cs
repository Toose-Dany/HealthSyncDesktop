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

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var (success, apiUser, error) = await MainWindow.Api.Login(username, password);

            if (success && apiUser != null)
            {
                var user = ConvertToLocalUser(apiUser);
                MainWindow.Instance.CurrentUser = user;
                MainWindow.Instance.LoadUserData();
                MainWindow.Instance.ShowMainContent();
                MessageBox.Show($"Добро пожаловать, {user.Username}!", "Успешно",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"Неверный логин или пароль!\n\nОшибка: {error}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.NavigateToPage(new RegisterPage());
        }

        private User ConvertToLocalUser(UserApiModel apiUser)
        {
            return new User
            {
                Id = apiUser.id,
                Username = apiUser.username,
                Email = apiUser.email,
                Height = apiUser.height,
                Weight = apiUser.weight,
                Age = apiUser.age,
                SyncCoins = apiUser.sync_coins,
                Steps = apiUser.steps,
                Water = apiUser.water,
                Sleep = apiUser.sleep,
                HeartRate = apiUser.heart_rate,
                Systolic = apiUser.systolic,
                Diastolic = apiUser.diastolic,
                StepsGoal = apiUser.steps_goal,
                WaterGoal = apiUser.water_goal,
                SleepGoal = apiUser.sleep_goal,
                CaloriesGoal = apiUser.calories_goal,
                City = apiUser.city ?? "Moscow",
                Theme = apiUser.theme ?? "Light",
                Gender = apiUser.gender == "male" ? "Мужской" : "Женский",
                NotificationsEnabled = apiUser.notifications_enabled,
                AutoSync = apiUser.auto_sync,
                DailyReminder = apiUser.daily_reminder,
                ReminderTime = apiUser.reminder_time ?? "20:00"
            };
        }
    }
}