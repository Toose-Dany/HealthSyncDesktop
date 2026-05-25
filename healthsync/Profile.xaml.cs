using System;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;

namespace HealthSync
{
    public partial class ProfilePage : Page
    {
        private bool isSaveClicked = false;

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

                if (user.Gender == "Мужской")
                    GenderBox.SelectedIndex = 0;
                else if (user.Gender == "Женский")
                    GenderBox.SelectedIndex = 1;
                else
                    GenderBox.SelectedIndex = 0;
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (isSaveClicked) return;
            isSaveClicked = true;

            var user = MainWindow.Instance.CurrentUser;
            if (user != null)
            {
                if (!string.IsNullOrWhiteSpace(UsernameBox.Text))
                    user.Username = UsernameBox.Text.Trim();
                else
                {
                    MainWindow.Instance.ShowNotification("Имя пользователя не может быть пустым!", "Ошибка");
                    isSaveClicked = false;
                    return;
                }

                if (!string.IsNullOrWhiteSpace(EmailBox.Text) && EmailBox.Text.Contains("@"))
                    user.Email = EmailBox.Text.Trim();
                else
                {
                    MainWindow.Instance.ShowNotification("Введите корректный email!", "Ошибка");
                    isSaveClicked = false;
                    return;
                }

                if (GenderBox.SelectedItem != null)
                {
                    var selectedGender = GenderBox.SelectedItem as ComboBoxItem;
                    if (selectedGender != null)
                        user.Gender = selectedGender.Content.ToString();
                }

                if (int.TryParse(AgeBox.Text, out int age) && age >= 10 && age <= 120)
                    user.Age = age;
                else
                {
                    MainWindow.Instance.ShowNotification("Введите корректный возраст (10-120 лет)!", "Ошибка");
                    isSaveClicked = false;
                    return;
                }

                if (double.TryParse(HeightBox.Text, out double height) && height >= 100 && height <= 250)
                {
                    user.Height = height;
                    await MainWindow.Api.UpdateHeight(user.Id, height);
                }
                else
                {
                    MainWindow.Instance.ShowNotification("Введите корректный рост (100-250 см)!", "Ошибка");
                    isSaveClicked = false;
                    return;
                }

                if (double.TryParse(WeightBox.Text, out double weight) && weight >= 20 && weight <= 300)
                {
                    user.Weight = weight;
                    await MainWindow.Api.UpdateWeight(user.Id, weight);
                }
                else
                {
                    MainWindow.Instance.ShowNotification("Введите корректный вес (20-300 кг)!", "Ошибка");
                    isSaveClicked = false;
                    return;
                }

                UserManager.UpdateUser(user);
                MainWindow.Instance.LoadUserData();
                MainWindow.Instance.UpdateUI();

                MainWindow.Instance.ShowNotification("Профиль успешно обновлен!", "Успешно");
            }

            await Task.Delay(500);
            isSaveClicked = false;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.ShowMainContent();
        }
    }
}