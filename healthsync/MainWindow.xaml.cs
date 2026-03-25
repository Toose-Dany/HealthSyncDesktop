using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.Threading.Tasks;

namespace HealthSync
{
    public partial class MainWindow : Window
    {
        public static MainWindow Instance;
        public User CurrentUser { get; set; }

        // Основные показатели
        public int syncCoins = 150;
        public double weight = 72.5;
        public double height = 178; // в см

        // Пульс и давление
        public int heartRate = 68;
        public int systolic = 118;
        public int diastolic = 75;

        // Дневные метрики - с нуля
        public int steps = 0;
        public int stepsGoal = 10000;
        public double water = 0;
        public double waterGoal = 2.5;
        public double sleep = 0;
        public double sleepGoal = 8.0;
        public int calories = 0;

        // История для графика
        private int[] weekSteps = { 4200, 3800, 5100, 4700, 3450, 6200, 5800 };

        private Random random = new Random();

        // Флаги для предотвращения двойного нажатия
        private bool isStepsClicked = false;
        private bool isWaterClicked = false;
        private bool isSleepClicked = false;
        private bool isPressureClicked = false;
        private bool isLogSleepClicked = false;
        private bool isAddVitalsClicked = false;
        private bool isEditWeightClicked = false;
        private bool isEditHeightClicked = false;
        private bool isWaterGoalClicked = false;
        private bool isStepsGoalClicked = false;
        private bool isSleepGoalClicked = false;

        // КОНСТРУКТОР MainWindow - вызывается при запуске приложения
        public MainWindow()
        {
            InitializeComponent();
            Instance = this;

            // Создаем дефолтного пользователя если нет пользователей
            UserManager.CreateDefaultUserIfNeeded();

            // Скрываем основной контент и показываем страницу входа
            MainContent.Visibility = Visibility.Collapsed;
            MainFrame.Visibility = Visibility.Visible;
            MainFrame.Navigate(new LoginPage());

            InitializeData();
        }

        private void InitializeData()
        {
            CurrentDateText.Text = DateTime.Now.ToString("d MMMM yyyy");

            // Добавляем обработчики для всех кнопок
            EditWeightButton.Click += EditWeight_Click;
            EditHeightButton.Click += EditHeight_Click;
            AddVitalsButton.Click += AddVitals_Click;
            LogSleepButton.Click += LogSleep_Click;

            // Кнопки быстрого ввода (верхние)
            QuickStepsButton.Click += QuickSteps_Click;
            QuickWaterButton.Click += QuickWater_Click;
            QuickPressureButton.Click += QuickPressure_Click;

            // Кнопки в целях (нижние)
            AddWaterButton.Click += AddWater_Click;
            AddStepsButton.Click += AddSteps_Click;
            AddSleepButton.Click += AddSleep_Click;

            // Добавляем клик для настройки целей
            WaterGoalText.MouseDown += WaterGoalText_MouseDown;
            StepsGoalText.MouseDown += StepsGoalText_MouseDown;
            SleepGoalText.MouseDown += SleepGoalText_MouseDown;

            // Обработчики для кликабельных текстов из XAML
            BMIText.MouseDown += BMIText_MouseDown;
            BloodPressureText.MouseDown += BloodPressureText_MouseDown;
        }

        public void LoadUserData()
        {
            if (CurrentUser != null)
            {
                stepsGoal = CurrentUser.StepsGoal;
                waterGoal = CurrentUser.WaterGoal;
                sleepGoal = CurrentUser.SleepGoal;
                syncCoins = CurrentUser.SyncCoins;
                height = CurrentUser.Height;
                weight = CurrentUser.Weight;
                heartRate = CurrentUser.HeartRate;
                systolic = CurrentUser.Systolic;
                diastolic = CurrentUser.Diastolic;
                steps = CurrentUser.Steps;
                water = CurrentUser.Water;
                sleep = CurrentUser.Sleep;
                calories = CurrentUser.Calories;

                UpdateUI();
                LoadHistory();
                UpdateGraph();
                ApplyTheme(CurrentUser.Theme);
            }
        }

        public void UpdateUI()
        {
            // Если есть пользователь, используем его настройки
            if (CurrentUser != null)
            {
                stepsGoal = CurrentUser.StepsGoal;
                waterGoal = CurrentUser.WaterGoal;
                sleepGoal = CurrentUser.SleepGoal;
                syncCoins = CurrentUser.SyncCoins;
                height = CurrentUser.Height;
                weight = CurrentUser.Weight;
            }

            // SyncCoin
            HeaderSyncCoinText.Text = syncCoins.ToString();

            // Вес и рост
            WeightText.Text = weight.ToString("F1");
            HeightText.Text = height.ToString();

            // ИМТ
            double bmi = weight / ((height / 100) * (height / 100));
            BMIText.Text = bmi.ToString("F1");

            if (bmi < 18.5)
            {
                BMICategoryText.Text = "Недостаточный вес ⚠️";
                BMICategoryText.Foreground = new SolidColorBrush(Color.FromRgb(255, 152, 0));
            }
            else if (bmi < 25)
            {
                BMICategoryText.Text = "Нормальный вес ✅";
                BMICategoryText.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80));
            }
            else if (bmi < 30)
            {
                BMICategoryText.Text = "Избыточный вес ⚠️";
                BMICategoryText.Foreground = new SolidColorBrush(Color.FromRgb(255, 152, 0));
            }
            else
            {
                BMICategoryText.Text = "Ожирение ❌";
                BMICategoryText.Foreground = new SolidColorBrush(Color.FromRgb(244, 67, 54));
            }

            // Сердце
            HeartRateText.Text = heartRate.ToString();
            BloodPressureText.Text = $"{systolic}/{diastolic}";
            UpdatePressureColor();

            // Сон
            SleepHoursText.Text = sleep.ToString("F1");
            int sleepScore = CalculateSleepScore();
            SleepScoreText.Text = sleepScore.ToString();
            UpdateSleepColor();

            // Цели - отображаем текущие значения
            WaterGoalText.Text = $"{water:F1}/{waterGoal} л";
            WaterProgressBar.Value = (water / waterGoal) * 100;

            StepsGoalText.Text = $"{steps:N0}/{stepsGoal:N0}";
            StepsProgressBar.Value = ((double)steps / stepsGoal) * 100;

            SleepGoalText.Text = $"{sleep:F1}/{sleepGoal} ч";
            SleepProgressBar.Value = (sleep / sleepGoal) * 100;

            // Футер
            FooterStepsText.Text = steps.ToString("N0");
            FooterCaloriesText.Text = calories.ToString("N0");
            FooterWaterText.Text = water.ToString("F1") + " л";
            FooterSleepText.Text = sleep.ToString("F1") + " ч";

            // Инсайт дня
            UpdateDailyInsight();

            // Биологический возраст
            UpdateBioAge();
        }

        private void UpdatePressureColor()
        {
            if (systolic < 90)
                BloodPressureText.Foreground = new SolidColorBrush(Color.FromRgb(255, 152, 0));
            else if (systolic < 120)
                BloodPressureText.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80));
            else if (systolic < 130)
                BloodPressureText.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80));
            else if (systolic < 140)
                BloodPressureText.Foreground = new SolidColorBrush(Color.FromRgb(255, 152, 0));
            else
                BloodPressureText.Foreground = new SolidColorBrush(Color.FromRgb(244, 67, 54));
        }

        private void UpdateSleepColor()
        {
            if (sleep == 0)
                SleepHoursText.Foreground = new SolidColorBrush(Color.FromRgb(156, 156, 156));
            else if (sleep >= 7 && sleep <= 8)
                SleepHoursText.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80));
            else if (sleep >= 6 && sleep < 7)
                SleepHoursText.Foreground = new SolidColorBrush(Color.FromRgb(33, 150, 243));
            else if (sleep > 8)
                SleepHoursText.Foreground = new SolidColorBrush(Color.FromRgb(255, 152, 0));
            else
                SleepHoursText.Foreground = new SolidColorBrush(Color.FromRgb(244, 67, 54));
        }

        private int CalculateSleepScore()
        {
            if (sleep == 0) return 0;
            if (sleep >= 7 && sleep <= 8) return 85 + random.Next(10);
            if (sleep >= 6 && sleep < 7) return 70 + random.Next(10);
            if (sleep > 8) return 75 + random.Next(10);
            return 50 + random.Next(15);
        }

        private string GetSleepDescription()
        {
            if (sleep == 0) return "Сон не записан";
            if (sleep >= 7.5) return "Отличный сон! ✨";
            if (sleep >= 7) return "Хороший сон 😊";
            if (sleep >= 6) return "Нормальный сон 😐";
            if (sleep >= 5) return "Мало сна 😴";
            return "Очень мало сна 🚨";
        }

        private string GetPressureDescription()
        {
            if (systolic < 90)
                return "Пониженное давление";
            else if (systolic < 120)
                return "Оптимальное давление";
            else if (systolic < 130)
                return "Нормальное давление";
            else if (systolic < 140)
                return "Высокое нормальное";
            else if (systolic < 160)
                return "Повышенное давление";
            else
                return "Высокое давление";
        }

        private void UpdateDailyInsight()
        {
            string[] insights = {
                "❤️ Пульс в норме. Для здоровья сердца проходите 8000+ шагов ежедневно.",
                "💧 Вам нужно выпить " + (waterGoal - water).ToString("F1") + " л воды. Начните прямо сейчас!",
                "👣 Сегодня вы прошли " + steps.ToString("N0") + " шагов. До цели осталось " + (stepsGoal - steps).ToString("N0"),
                "😴 " + GetSleepDescription() + " (" + sleep.ToString("F1") + " ч)",
                "⚖️ Ваш ИМТ в норме. Так держать!",
                "🫀 " + GetPressureDescription() + " - " + systolic + "/" + diastolic
            };

            DailyInsightText.Text = "💡 " + insights[random.Next(insights.Length)];
        }

        private void UpdateBioAge()
        {
            int actualAge = 28;
            int bioAge = actualAge;

            if (steps > 7000 && sleep >= 7 && heartRate < 70 && systolic < 120)
                bioAge = actualAge - 2;
            else if (steps < 3000 || sleep < 6 || heartRate > 80 || systolic > 130)
                bioAge = actualAge + 3;

            BioAgeText.Text = $"Биологический возраст: {bioAge}";
        }

        private void UpdateGraph()
        {
            try
            {
                var grid = StepsGraphGrid;
                if (grid != null)
                {
                    for (int i = 0; i < Math.Min(weekSteps.Length, 7); i++)
                    {
                        var border = grid.Children[i] as Border;
                        if (border != null)
                        {
                            double maxStep = 10000;
                            double heightPercent = (weekSteps[i] / maxStep) * 100;
                            border.Height = Math.Max(20, heightPercent);
                            border.ToolTip = $"{weekSteps[i]:N0} шагов";
                        }
                    }
                }
            }
            catch { }
        }

        private void LoadHistory()
        {
            var history = new List<HistoryItem>
            {
                new HistoryItem { Icon = "❤️", Title = "Пульс", Time = "09:30", Value = $"{heartRate} уд/мин", Color = "#F44336" },
                new HistoryItem { Icon = "🫀", Title = "Давление", Time = "09:30", Value = $"{systolic}/{diastolic}", Color = "#2196F3" },
                new HistoryItem { Icon = "⚖️", Title = "Вес", Time = "Вчера", Value = $"{weight} кг", Color = "#4CAF50" },
                new HistoryItem { Icon = "😴", Title = "Сон", Time = "Сегодня", Value = $"{sleep} ч", Color = "#5C6BC0" }
            };

            HistoryListBox.ItemsSource = history;
        }

        public void ShowNotification(string message, string title = "Информация")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public bool ShowConfirmation(string message)
        {
            return MessageBox.Show(message, "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }

        public void ApplyTheme(string theme)
        {
            if (theme == "Dark")
            {
                this.Background = new SolidColorBrush(Color.FromRgb(30, 30, 30));
                MainContent.Background = new SolidColorBrush(Color.FromRgb(30, 30, 30));
            }
            else
            {
                this.Background = new SolidColorBrush(Color.FromRgb(245, 245, 245));
                MainContent.Background = new SolidColorBrush(Color.FromRgb(245, 245, 245));
            }
        }

        public void SaveCurrentUser()
        {
            if (CurrentUser != null)
            {
                // Обновляем данные пользователя из текущих показателей
                CurrentUser.StepsGoal = stepsGoal;
                CurrentUser.WaterGoal = waterGoal;
                CurrentUser.SleepGoal = sleepGoal;
                CurrentUser.SyncCoins = syncCoins;
                CurrentUser.Height = height;
                CurrentUser.Weight = weight;
                CurrentUser.HeartRate = heartRate;
                CurrentUser.Systolic = systolic;
                CurrentUser.Diastolic = diastolic;
                CurrentUser.Steps = steps;
                CurrentUser.Water = water;
                CurrentUser.Sleep = sleep;
                CurrentUser.Calories = calories;

                UserManager.UpdateUser(CurrentUser);
            }
        }

        // Настройка целей по клику
        private async void WaterGoalText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isWaterGoalClicked) return;
            isWaterGoalClicked = true;

            string input = Microsoft.VisualBasic.Interaction.InputBox("Введите вашу цель по воде (в литрах):",
                "Настройка цели", waterGoal.ToString("F1"));

            if (double.TryParse(input, out double newGoal) && newGoal > 0 && newGoal < 10)
            {
                waterGoal = newGoal;
                if (CurrentUser != null) CurrentUser.WaterGoal = newGoal;
                UpdateUI();
                ShowNotification($"💧 Цель по воде обновлена: {waterGoal:F1} л", "Успешно");
            }
            else
            {
                ShowNotification("Пожалуйста, введите корректную цель (0-10 л)", "Ошибка");
            }

            await Task.Delay(500);
            isWaterGoalClicked = false;
        }

        private async void StepsGoalText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isStepsGoalClicked) return;
            isStepsGoalClicked = true;

            string input = Microsoft.VisualBasic.Interaction.InputBox("Введите вашу цель по шагам:",
                "Настройка цели", stepsGoal.ToString());

            if (int.TryParse(input, out int newGoal) && newGoal > 0 && newGoal < 50000)
            {
                stepsGoal = newGoal;
                if (CurrentUser != null) CurrentUser.StepsGoal = newGoal;
                UpdateUI();
                ShowNotification($"👣 Цель по шагам обновлена: {stepsGoal:N0} шагов", "Успешно");
            }
            else
            {
                ShowNotification("Пожалуйста, введите корректную цель (0-50000 шагов)", "Ошибка");
            }

            await Task.Delay(500);
            isStepsGoalClicked = false;
        }

        private async void SleepGoalText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isSleepGoalClicked) return;
            isSleepGoalClicked = true;

            string input = Microsoft.VisualBasic.Interaction.InputBox("Введите вашу цель по сну (в часах):",
                "Настройка цели", sleepGoal.ToString("F1"));

            if (double.TryParse(input, out double newGoal) && newGoal > 0 && newGoal < 24)
            {
                sleepGoal = newGoal;
                if (CurrentUser != null) CurrentUser.SleepGoal = newGoal;
                UpdateUI();
                ShowNotification($"😴 Цель по сну обновлена: {sleepGoal:F1} ч", "Успешно");
            }
            else
            {
                ShowNotification("Пожалуйста, введите корректную цель (0-24 часа)", "Ошибка");
            }

            await Task.Delay(500);
            isSleepGoalClicked = false;
        }

        // Обработчик для клика по ИМТ
        private async void BMIText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            double bmi = weight / ((height / 100) * (height / 100));
            string message = $"Ваш ИМТ: {bmi:F1}\n\n";
            message += $"Категория: {BMICategoryText.Text}\n\n";
            message += "РЕКОМЕНДАЦИИ:\n";

            if (bmi < 18.5)
                message += "• Увеличьте калорийность рациона\n• Добавьте белки и полезные жиры\n• Пейте больше воды";
            else if (bmi < 25)
                message += "• Отличный показатель!\n• Продолжайте в том же духе\n• Поддерживайте активный образ жизни";
            else if (bmi < 30)
                message += "• Увеличьте физическую активность\n• Уменьшите потребление сахара\n• Добавьте больше овощей";
            else
                message += "• Обратитесь к врачу\n• Разработайте план снижения веса\n• Начните с небольших прогулок";

            MessageBox.Show(message, "Анализ ИМТ", MessageBoxButton.OK, MessageBoxImage.Information);

            await Task.Delay(500);
        }

        // Обработчик для клика по давлению
        private async void BloodPressureText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string message = $"Ваше давление: {systolic}/{diastolic}\n";
            message += $"Пульс: {heartRate} уд/мин\n\n";
            message += $"Статус: {GetPressureDescription()}\n\n";
            message += "РЕКОМЕНДАЦИИ:\n";

            if (systolic < 90)
                message += "• Пейте больше воды\n• Ешьте чаще, но меньше\n• Выпейте кофе или чай\n• Отдохните";
            else if (systolic < 120)
                message += "• Отличное давление!\n• Продолжайте в том же духе\n• Поддерживайте здоровый образ жизни";
            else if (systolic < 130)
                message += "• Нормальное давление\n• Следите за питанием\n• Ограничьте соль";
            else if (systolic < 140)
                message += "• Уменьшите соль\n• Больше двигайтесь\n• Контролируйте вес\n• Избегайте стресса";
            else if (systolic < 160)
                message += "• Обратитесь к врачу\n• Исключите соль\n• Откажитесь от алкоголя\n• Больше отдыхайте";
            else
                message += "• СРОЧНО к врачу!\n• Вызовите скорую при ухудшении\n• Примите прописанные лекарства";

            MessageBox.Show(message, "Анализ давления", MessageBoxButton.OK,
                          systolic >= 140 ? MessageBoxImage.Warning : MessageBoxImage.Information);

            await Task.Delay(500);
        }

        // Обработчики кнопок
        private async void EditWeight_Click(object sender, RoutedEventArgs e)
        {
            if (isEditWeightClicked) return;
            isEditWeightClicked = true;

            string input = Microsoft.VisualBasic.Interaction.InputBox("Введите новый вес (кг):", "Изменение веса", weight.ToString("F1"));
            if (double.TryParse(input, out double newWeight) && newWeight > 20 && newWeight < 300)
            {
                weight = newWeight;
                if (CurrentUser != null) CurrentUser.Weight = newWeight;
                UpdateUI();
                ShowNotification($"⚖️ Вес обновлен: {weight:F1} кг", "Успешно");
            }
            else
            {
                ShowNotification("Пожалуйста, введите корректный вес (20-300 кг)", "Ошибка");
            }

            await Task.Delay(500);
            isEditWeightClicked = false;
        }

        private async void EditHeight_Click(object sender, RoutedEventArgs e)
        {
            if (isEditHeightClicked) return;
            isEditHeightClicked = true;

            string input = Microsoft.VisualBasic.Interaction.InputBox("Введите новый рост (см):", "Изменение роста", height.ToString());
            if (double.TryParse(input, out double newHeight) && newHeight > 100 && newHeight < 250)
            {
                height = newHeight;
                if (CurrentUser != null) CurrentUser.Height = newHeight;
                UpdateUI();
                ShowNotification($"📏 Рост обновлен: {height} см", "Успешно");
            }
            else
            {
                ShowNotification("Пожалуйста, введите корректный рост (100-250 см)", "Ошибка");
            }

            await Task.Delay(500);
            isEditHeightClicked = false;
        }

        private async void AddVitals_Click(object sender, RoutedEventArgs e)
        {
            if (isAddVitalsClicked) return;
            isAddVitalsClicked = true;

            string pulseInput = Microsoft.VisualBasic.Interaction.InputBox("Введите пульс (уд/мин):", "Пульс", heartRate.ToString());
            string bpInput = Microsoft.VisualBasic.Interaction.InputBox("Введите давление (сист/диаст):", "Давление", $"{systolic}/{diastolic}");

            if (int.TryParse(pulseInput, out int newPulse) && newPulse > 30 && newPulse < 200)
            {
                heartRate = newPulse;

                string[] bp = bpInput.Split('/');
                if (bp.Length == 2 && int.TryParse(bp[0], out int sys) && int.TryParse(bp[1], out int dias))
                {
                    if (sys > 50 && sys < 250 && dias > 30 && dias < 200)
                    {
                        systolic = sys;
                        diastolic = dias;
                    }
                }

                syncCoins += 5;
                if (CurrentUser != null) CurrentUser.SyncCoins = syncCoins;
                UpdateUI();

                string status = GetPressureDescription();
                string recommendation = "";

                if (systolic >= 140)
                    recommendation = "\n\n⚠️ Внимание! Повышенное давление. Рекомендуется обратиться к врачу.";
                else if (systolic < 90)
                    recommendation = "\n\nПониженное давление. Пейте больше воды.";

                ShowNotification($"❤️ Показатели сохранены!\nДавление: {systolic}/{diastolic} - {status}{recommendation}\n+5 SyncCoin", "Успешно");
            }

            await Task.Delay(500);
            isAddVitalsClicked = false;
        }

        private async void LogSleep_Click(object sender, RoutedEventArgs e)
        {
            if (isLogSleepClicked) return;
            isLogSleepClicked = true;

            string input = Microsoft.VisualBasic.Interaction.InputBox("Сколько часов вы спали?",
                "Запись сна", sleep.ToString("F1"));

            if (double.TryParse(input, out double newSleep) && newSleep > 0 && newSleep <= 24)
            {
                sleep = Math.Min(sleepGoal, newSleep);

                syncCoins += 3;
                if (CurrentUser != null) CurrentUser.SyncCoins = syncCoins;
                UpdateUI();

                string description = GetSleepDescription();
                string recommendation = "";

                if (sleep < 6)
                    recommendation = "\n\n😴 Старайтесь спать больше - это важно для здоровья!";
                else if (sleep > 9)
                    recommendation = "\n\nВозможно, вы слишком много спите. Попробуйте просыпаться раньше.";

                ShowNotification($"😴 Сон записан: {sleep} ч - {description}{recommendation}\n+3 SyncCoin", "Успешно");
            }
            else
            {
                ShowNotification("Пожалуйста, введите корректное количество часов (1-24)", "Ошибка");
            }

            await Task.Delay(500);
            isLogSleepClicked = false;
        }

        // Кнопки быстрого ввода (верхние)
        private async void QuickSteps_Click(object sender, RoutedEventArgs e)
        {
            if (isStepsClicked) return;
            isStepsClicked = true;

            steps = Math.Min(stepsGoal, steps + 500);
            calories += 30;
            syncCoins += 1;
            if (CurrentUser != null) CurrentUser.SyncCoins = syncCoins;

            UpdateUI();

            int remaining = stepsGoal - steps;
            if (remaining <= 0)
                ShowNotification("🎉 Поздравляем! Вы выполнили норму шагов!", "Поздравляем!");
            else
                ShowNotification($"👣 +500 шагов! Осталось {remaining} шагов\n+1 SyncCoin", "Успешно");

            await Task.Delay(500);
            isStepsClicked = false;
        }

        private async void QuickWater_Click(object sender, RoutedEventArgs e)
        {
            if (isWaterClicked) return;
            isWaterClicked = true;

            water = Math.Min(waterGoal, water + 0.25);
            syncCoins += 1;
            if (CurrentUser != null) CurrentUser.SyncCoins = syncCoins;

            UpdateUI();

            double remaining = waterGoal - water;
            if (remaining <= 0)
                ShowNotification("💧 Отлично! Вы выполнили норму воды!", "Поздравляем!");
            else
                ShowNotification($"💧 +250 мл воды! Осталось {remaining:F1} л\n+1 SyncCoin", "Успешно");

            await Task.Delay(500);
            isWaterClicked = false;
        }

        private async void QuickPressure_Click(object sender, RoutedEventArgs e)
        {
            if (isPressureClicked) return;
            isPressureClicked = true;

            systolic = random.Next(110, 125);
            diastolic = random.Next(70, 80);
            heartRate = random.Next(60, 75);
            syncCoins += 2;
            if (CurrentUser != null) CurrentUser.SyncCoins = syncCoins;
            UpdateUI();

            string status = GetPressureDescription();
            ShowNotification($"🫀 Давление: {systolic}/{diastolic} - {status}\n+2 SyncCoin", "Успешно");

            await Task.Delay(500);
            isPressureClicked = false;
        }

        // Кнопки в целях (нижние)
        private async void AddSteps_Click(object sender, RoutedEventArgs e)
        {
            if (isStepsClicked) return;
            isStepsClicked = true;

            steps = Math.Min(stepsGoal, steps + 500);
            calories += 30;
            UpdateUI();

            int remaining = stepsGoal - steps;
            if (remaining <= 0)
                ShowNotification("🎉 Поздравляем! Вы выполнили норму шагов!", "Поздравляем!");
            else
                ShowNotification($"👣 +500 шагов! Осталось {remaining} шагов", "Успешно");

            await Task.Delay(500);
            isStepsClicked = false;
        }

        private async void AddWater_Click(object sender, RoutedEventArgs e)
        {
            if (isWaterClicked) return;
            isWaterClicked = true;

            water = Math.Min(waterGoal, water + 0.25);
            UpdateUI();

            double remaining = waterGoal - water;
            if (remaining <= 0)
                ShowNotification("💧 Отлично! Вы выполнили норму воды!", "Поздравляем!");
            else
                ShowNotification($"💧 +250 мл воды! Осталось {remaining:F1} л", "Успешно");

            await Task.Delay(500);
            isWaterClicked = false;
        }

        private async void AddSleep_Click(object sender, RoutedEventArgs e)
        {
            if (isSleepClicked) return;
            isSleepClicked = true;

            double newSleep = sleep + 1;

            if (newSleep <= sleepGoal)
            {
                sleep = newSleep;
                ShowNotification($"😴 +1 час сна! Всего {sleep:F1} из {sleepGoal} ч", "Успешно");
            }
            else
            {
                double added = sleepGoal - sleep;
                sleep = sleepGoal;
                ShowNotification($"🎉 Достигнута цель по сну! {sleepGoal} ч\n😴 +{added:F1} ч добавлено", "Поздравляем!");
            }

            UpdateUI();

            await Task.Delay(500);
            isSleepClicked = false;
        }

        // ==================== МЕТОДЫ ДЛЯ МЕНЮ И НАВИГАЦИИ ====================

        public void NavigateToPage(Page page)
        {
            MainContent.Visibility = Visibility.Collapsed;
            MainFrame.Visibility = Visibility.Visible;
            MainFrame.Navigate(page);
        }

        public void ShowMainContent()
        {
            // Сохраняем текущего пользователя
            SaveCurrentUser();

            MainFrame.Visibility = Visibility.Collapsed;
            MainContent.Visibility = Visibility.Visible;

            // Загружаем данные пользователя
            if (CurrentUser != null)
            {
                stepsGoal = CurrentUser.StepsGoal;
                waterGoal = CurrentUser.WaterGoal;
                sleepGoal = CurrentUser.SleepGoal;
                syncCoins = CurrentUser.SyncCoins;
                height = CurrentUser.Height;
                weight = CurrentUser.Weight;
                heartRate = CurrentUser.HeartRate;
                systolic = CurrentUser.Systolic;
                diastolic = CurrentUser.Diastolic;
                steps = CurrentUser.Steps;
                water = CurrentUser.Water;
                sleep = CurrentUser.Sleep;
                calories = CurrentUser.Calories;
            }

            UpdateUI();
            LoadHistory();
            UpdateGraph();
        }

        // Обработчики меню
        private void Home_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUser != null)
                ShowMainContent();
            else
                NavigateToPage(new LoginPage());
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUser != null)
                NavigateToPage(new ProfilePage());
            else
                NavigateToPage(new LoginPage());
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUser != null)
                NavigateToPage(new SettingsPage());
            else
                NavigateToPage(new LoginPage());
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(new HelpPage());
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }

    // Вспомогательный класс для истории
    public class HistoryItem
    {
        public string Icon { get; set; }
        public string Title { get; set; }
        public string Time { get; set; }
        public string Value { get; set; }
        public string Color { get; set; }
    }
}