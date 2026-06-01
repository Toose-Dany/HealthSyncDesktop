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

                // Устанавливаем пол в комбобоксе
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
            if (user == null)
            {
                isSaveClicked = false;
                return;
            }

            // 1. Сохраняем имя пользователя
            if (!string.IsNullOrWhiteSpace(UsernameBox.Text))
                user.Username = UsernameBox.Text.Trim();
            else
            {
                MainWindow.Instance.ShowNotification("Имя пользователя не может быть пустым!", "Ошибка");
                isSaveClicked = false;
                return;
            }

            // 2. Сохраняем email
            string newEmail = EmailBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(newEmail))
            {
                MainWindow.Instance.ShowNotification("Email не может быть пустым!", "Ошибка");
                isSaveClicked = false;
                return;
            }

            if (!newEmail.Contains("@") || !newEmail.Contains("."))
            {
                MainWindow.Instance.ShowNotification("Введите корректный email (пример: name@mail.com)!", "Ошибка");
                isSaveClicked = false;
                return;
            }
            user.Email = newEmail;

            // 3. Сохраняем пол (ГЛАВНОЕ - ЗАПОМИНАЕМ ВЫБРАННОЕ ЗНАЧЕНИЕ)
            string selectedGender = "Мужской";
            if (GenderBox.SelectedItem != null)
            {
                var selectedGenderItem = GenderBox.SelectedItem as ComboBoxItem;
                if (selectedGenderItem != null)
                {
                    selectedGender = selectedGenderItem.Content.ToString();
                }
            }

            // 4. Сохраняем возраст
            if (int.TryParse(AgeBox.Text, out int age) && age >= 10 && age <= 120)
                user.Age = age;
            else
            {
                MainWindow.Instance.ShowNotification("Введите корректный возраст (10-120 лет)!", "Ошибка");
                isSaveClicked = false;
                return;
            }

            // 5. Сохраняем рост
            if (double.TryParse(HeightBox.Text, out double height) && height >= 100 && height <= 250)
            {
                user.Height = height;
                try { await MainWindow.Api.UpdateHeight(user.Id, height); } catch { }
            }
            else
            {
                MainWindow.Instance.ShowNotification("Введите корректный рост (100-250 см)!", "Ошибка");
                isSaveClicked = false;
                return;
            }

            // 6. Сохраняем вес
            if (double.TryParse(WeightBox.Text, out double weight) && weight >= 20 && weight <= 300)
            {
                user.Weight = weight;
                try { await MainWindow.Api.UpdateWeight(user.Id, weight); } catch { }
            }
            else
            {
                MainWindow.Instance.ShowNotification("Введите корректный вес (20-300 кг)!", "Ошибка");
                isSaveClicked = false;
                return;
            }

            // 7. ОТПРАВЛЯЕМ НА СЕРВЕР
            try
            {
                string genderForApi = selectedGender == "Мужской" ? "male" : "female";
                bool success = await MainWindow.Api.UpdateProfile(user.Id, user.Username, user.Email, user.Age, genderForApi);

                if (success)
                {
                    // 8. ВАЖНО! Обновляем пол у локального пользователя
                    user.Gender = selectedGender;

                    // 9. Сохраняем локально
                    UserManager.UpdateUser(user);

                    // 10. Обновляем интерфейс главного окна
                    MainWindow.Instance.LoadUserData();
                    MainWindow.Instance.UpdateUI();

                    MainWindow.Instance.ShowNotification("Профиль успешно обновлен!", "Успешно");
                }
                else
                {
                    MainWindow.Instance.ShowNotification("Ошибка при сохранении на сервере!", "Ошибка");
                }
            }
            catch (Exception ex)
            {
                MainWindow.Instance.ShowNotification($"Ошибка: {ex.Message}", "Ошибка");
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