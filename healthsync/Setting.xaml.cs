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
                // Цели
                StepsGoalBox.Text = user.StepsGoal.ToString();
                WaterGoalBox.Text = user.WaterGoal.ToString("F1");
                SleepGoalBox.Text = user.SleepGoal.ToString("F1");

                // Уведомления
                NotificationsCheckBox.IsChecked = user.NotificationsEnabled;
                DailyReminderCheckBox.IsChecked = user.DailyReminder;

                // Время напоминания
                ReminderTimeBox.Text = user.ReminderTime;

                // Единицы измерения
                if (user.UnitsSystem == "Imperial")
                    UnitsSystemBox.SelectedIndex = 1;
                else
                    UnitsSystemBox.SelectedIndex = 0;

                // Тема
                if (user.Theme == "Dark")
                    ThemeBox.SelectedIndex = 1;
                else
                    ThemeBox.SelectedIndex = 0;

                // Автосинхронизация
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

            // Сохраняем цели
            if (int.TryParse(StepsGoalBox.Text, out int stepsGoal) && stepsGoal > 0)
                user.StepsGoal = stepsGoal;
            else
                MessageBox.Show("Некорректное значение для шагов!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);

            if (double.TryParse(WaterGoalBox.Text, out double waterGoal) && waterGoal > 0)
                user.WaterGoal = waterGoal;
            else
                MessageBox.Show("Некорректное значение для воды!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);

            if (double.TryParse(SleepGoalBox.Text, out double sleepGoal) && sleepGoal > 0)
                user.SleepGoal = sleepGoal;
            else
                MessageBox.Show("Некорректное значение для сна!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);

            // Сохраняем уведомления
            user.NotificationsEnabled = NotificationsCheckBox.IsChecked ?? false;
            user.DailyReminder = DailyReminderCheckBox.IsChecked ?? false;
            user.ReminderTime = ReminderTimeBox.Text;

            // Сохраняем единицы измерения
            user.UnitsSystem = (UnitsSystemBox.SelectedItem as ComboBoxItem)?.Content.ToString().Contains("Метрическая") == true ? "Metric" : "Imperial";

            // Сохраняем тему
            string newTheme = (ThemeBox.SelectedItem as ComboBoxItem)?.Content.ToString().Contains("Темная") == true ? "Dark" : "Light";
            user.Theme = newTheme;

            // Сохраняем автосинхронизацию
            user.AutoSync = AutoSyncCheckBox.IsChecked ?? false;

            // СОХРАНЯЕМ В ФАЙЛ
            UserManager.UpdateUser(user);

            // Применяем тему
            MainWindow.Instance.ApplyTheme(newTheme);

            // Обновляем UI в главном окне
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
                NotificationsCheckBox.IsChecked = true;
                DailyReminderCheckBox.IsChecked = true;
                ReminderTimeBox.Text = "20:00";
                UnitsSystemBox.SelectedIndex = 0;
                ThemeBox.SelectedIndex = 0;
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
                // Сохраняем данные текущего пользователя
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

                // Очищаем текущего пользователя
                MainWindow.Instance.CurrentUser = null;

                // Переходим на страницу входа
                MainWindow.Instance.NavigateToPage(new LoginPage());

                MainWindow.Instance.ShowNotification("Вы вышли из аккаунта", "Информация");
            }

            await Task.Delay(500);
            isLogoutClicked = false;
        }
    }
}