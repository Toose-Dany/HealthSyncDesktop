using System;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;

namespace HealthSync
{
    public partial class SettingsPage : Page
    {
        private bool isSaveClicked = false;
        private bool isLogoutClicked = false;

        public SettingsPage()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            var user = MainWindow.Instance.CurrentUser;
            if (user != null)
            {
                StepsGoalBox.Text = user.StepsGoal.ToString();
                WaterGoalBox.Text = user.WaterGoal.ToString("F1");
                SleepGoalBox.Text = user.SleepGoal.ToString("F1");
                CityTextBox.Text = user.City ?? "Moscow";
                NotificationsCheckBox.IsChecked = user.NotificationsEnabled;
                DailyReminderCheckBox.IsChecked = user.DailyReminder;
                ReminderTimeBox.Text = user.ReminderTime;
                AutoSyncCheckBox.IsChecked = user.AutoSync;
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (isSaveClicked) return;
            isSaveClicked = true;

            var user = MainWindow.Instance.CurrentUser;
            if (user == null)
            {
                isSaveClicked = false;
                return;
            }

            if (int.TryParse(StepsGoalBox.Text, out int stepsGoal) && stepsGoal > 0)
                user.StepsGoal = stepsGoal;

            if (double.TryParse(WaterGoalBox.Text, out double waterGoal) && waterGoal > 0)
                user.WaterGoal = waterGoal;

            if (double.TryParse(SleepGoalBox.Text, out double sleepGoal) && sleepGoal > 0)
                user.SleepGoal = sleepGoal;

            user.City = CityTextBox.Text.Trim();
            if (string.IsNullOrEmpty(user.City))
                user.City = "Moscow";

            user.NotificationsEnabled = NotificationsCheckBox.IsChecked ?? false;
            user.DailyReminder = DailyReminderCheckBox.IsChecked ?? false;
            user.ReminderTime = ReminderTimeBox.Text;
            user.AutoSync = AutoSyncCheckBox.IsChecked ?? false;

            // Отправляем на сервер
            await MainWindow.Api.UpdateSettings(user.Id, user.City, user.Theme, user.NotificationsEnabled, user.AutoSync, user.DailyReminder, user.ReminderTime);
            await MainWindow.Api.UpdateGoals(user.Id, user.StepsGoal, user.WaterGoal, user.SleepGoal, user.CaloriesGoal);

            UserManager.UpdateUser(user);
            MainWindow.Instance.LoadWeather();
            MainWindow.Instance.UpdateUI();

            MainWindow.Instance.ShowNotification("Настройки успешно сохранены!", "Успешно");

            await Task.Delay(500);
            isSaveClicked = false;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.Instance.ShowConfirmation("Сбросить все настройки на значения по умолчанию?"))
            {
                StepsGoalBox.Text = "10000";
                WaterGoalBox.Text = "2.5";
                SleepGoalBox.Text = "8.0";
                CityTextBox.Text = "Moscow";
                NotificationsCheckBox.IsChecked = true;
                DailyReminderCheckBox.IsChecked = true;
                ReminderTimeBox.Text = "20:00";
                AutoSyncCheckBox.IsChecked = true;

                MainWindow.Instance.ShowNotification("Настройки сброшены! Не забудьте сохранить.");
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.ShowMainContent();
        }

        private async void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (isLogoutClicked) return;
            isLogoutClicked = true;

            if (MainWindow.Instance.ShowConfirmation("Вы уверены, что хотите выйти?"))
            {
                var user = MainWindow.Instance.CurrentUser;
                if (user != null)
                {
                    user.StepsGoal = MainWindow.Instance.stepsGoal;
                    user.WaterGoal = MainWindow.Instance.waterGoal;
                    user.SleepGoal = MainWindow.Instance.sleepGoal;
                    user.SyncCoins = MainWindow.Instance.syncCoins;
                    user.Height = MainWindow.Instance.height;
                    user.Weight = MainWindow.Instance.weight;
                    user.HeartRate = MainWindow.Instance.heartRate;
                    user.Systolic = MainWindow.Instance.systolic;
                    user.Diastolic = MainWindow.Instance.diastolic;
                    user.Steps = MainWindow.Instance.steps;
                    user.Water = MainWindow.Instance.water;
                    user.Sleep = MainWindow.Instance.sleep;
                    user.Calories = MainWindow.Instance.calories;

                    UserManager.UpdateUser(user);
                }

                MainWindow.Instance.CurrentUser = null;
                MainWindow.Instance.NavigateToPage(new LoginPage());
                MainWindow.Instance.ShowNotification("Вы вышли из аккаунта", "Информация");
            }

            await Task.Delay(500);
            isLogoutClicked = false;
        }
    }
}