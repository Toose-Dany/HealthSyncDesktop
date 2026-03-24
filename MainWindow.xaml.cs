using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Xml;

namespace HealthSync
{
    public partial class MainWindow : Window
    {
        // Основные показатели
        private int syncCoins = 150;
        private double weight = 72.5;
        private double height = 178; // в см

        // Пульс и давление
        private int heartRate = 68;
        private int systolic = 118;
        private int diastolic = 75;

        // Дневные метрики - с нуля
        private int steps = 0;
        private int stepsGoal = 10000;
        private double water = 0;
        private double waterGoal = 2.5;
        private double sleep = 0;
        private double sleepGoal = 8.0;
        private int calories = 0;

        // История для графика
        private int[] weekSteps = { 4200, 3800, 5100, 4700, 3450, 6200, 5800 };

        private Random random = new Random();

        // Флаги для предотвращения двойного нажатия
        private bool isStepsClicked = false;
        private bool isWaterClicked = false;

        // API сервис - ЗАКОММЕНТИРОВАНО
        // private ApiService _apiService;
        // private int _userId = 1;

        public MainWindow()
        {
            InitializeComponent();

            // Инициализация API - ЗАКОММЕНТИРОВАНО
            // _apiService = new ApiService();

            InitializeData();

            // Асинхронная загрузка данных с сервера - ЗАКОММЕНТИРОВАНО
            // _ = LoadDataFromServerAsync();

            UpdateUI();
            LoadHistory();
            UpdateGraph();
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

        // Метод загрузки с сервера - ЗАКОММЕНТИРОВАНО
        /*
        private async Task LoadDataFromServerAsync()
        {
            try
            {
                var metrics = await _apiService.GetTodayMetricsAsync(_userId);
                
                if (metrics != null)
                {
                    steps = metrics.steps;
                    water = metrics.water;
                    sleep = metrics.sleep;
                    calories = metrics.calories;
                    
                    if (metrics.heart_rate > 0)
                        heartRate = metrics.heart_rate;
                    if (metrics.systolic > 0)
                        systolic = metrics.systolic;
                    if (metrics.diastolic > 0)
                        diastolic = metrics.diastolic;
                }
                
                int serverBalance = await _apiService.GetSyncCoinBalanceAsync(_userId);
                if (serverBalance > 0)
                {
                    syncCoins = serverBalance;
                }
                
                Dispatcher.Invoke(() => UpdateUI());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки с сервера: {ex.Message}");
                Dispatcher.Invoke(() => 
                    ShowNotification("⚠️ Не удалось подключиться к серверу. Используются локальные данные."));
            }
        }
        */

        private void UpdateUI()
        {
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

            // Меняем цвет давления в зависимости от показаний
            UpdatePressureColor();

            // Сон
            SleepHoursText.Text = sleep.ToString("F1");
            int sleepScore = CalculateSleepScore();
            SleepScoreText.Text = sleepScore.ToString();

            // Меняем цвет сна в зависимости от качества
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
                BloodPressureText.Foreground = new SolidColorBrush(Color.FromRgb(255, 152, 0)); // Оранжевый
            else if (systolic < 120)
                BloodPressureText.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Зеленый
            else if (systolic < 130)
                BloodPressureText.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Зеленый
            else if (systolic < 140)
                BloodPressureText.Foreground = new SolidColorBrush(Color.FromRgb(255, 152, 0)); // Оранжевый
            else
                BloodPressureText.Foreground = new SolidColorBrush(Color.FromRgb(244, 67, 54)); // Красный
        }

        private void UpdateSleepColor()
        {
            if (sleep == 0)
                SleepHoursText.Foreground = new SolidColorBrush(Color.FromRgb(156, 156, 156)); // Серый
            else if (sleep >= 7 && sleep <= 8)
                SleepHoursText.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Зеленый
            else if (sleep >= 6 && sleep < 7)
                SleepHoursText.Foreground = new SolidColorBrush(Color.FromRgb(33, 150, 243)); // Синий
            else if (sleep > 8)
                SleepHoursText.Foreground = new SolidColorBrush(Color.FromRgb(255, 152, 0)); // Оранжевый
            else
                SleepHoursText.Foreground = new SolidColorBrush(Color.FromRgb(244, 67, 54)); // Красный
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

        private void ShowNotification(string message, bool isSuccess = true)
        {
            MessageBox.Show(message, isSuccess ? "Успешно" : "Информация",
                          MessageBoxButton.OK, isSuccess ? MessageBoxImage.Information : MessageBoxImage.Information);
        }

        // Настройка целей по клику
        private void WaterGoalText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox("Введите вашу цель по воде (в литрах):",
                "Настройка цели", waterGoal.ToString("F1"));

            if (double.TryParse(input, out double newGoal) && newGoal > 0 && newGoal < 10)
            {
                waterGoal = newGoal;
                UpdateUI();
                ShowNotification("💧 Цель по воде обновлена!");
            }
        }

        private void StepsGoalText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox("Введите вашу цель по шагам:",
                "Настройка цели", stepsGoal.ToString());

            if (int.TryParse(input, out int newGoal) && newGoal > 0 && newGoal < 50000)
            {
                stepsGoal = newGoal;
                UpdateUI();
                ShowNotification("👣 Цель по шагам обновлена!");
            }
        }

        private void SleepGoalText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox("Введите вашу цель по сну (в часах):",
                "Настройка цели", sleepGoal.ToString("F1"));

            if (double.TryParse(input, out double newGoal) && newGoal > 0 && newGoal < 24)
            {
                sleepGoal = newGoal;
                UpdateUI();
                ShowNotification("😴 Цель по сну обновлена!");
            }
        }

        // Обработчик для клика по ИМТ
        private void BMIText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            double bmi = weight / ((height / 100) * (height / 100));
            string message = $"Ваш ИМТ: {bmi:F1}\n\n";
            message += $"Категория: {BMICategoryText.Text}\n\n";
            message += "РЕКОМЕНДАЦИИ:\n";

            if (bmi < 18.5)
                message += "• Увеличьте калорийность рациона\n• Добавьте белки и полезные жиры";
            else if (bmi < 25)
                message += "• Отличный показатель!\n• Продолжайте в том же духе";
            else if (bmi < 30)
                message += "• Увеличьте физическую активность\n• Уменьшите потребление сахара";
            else
                message += "• Обратитесь к врачу\n• Разработайте план снижения веса";

            MessageBox.Show(message, "Анализ ИМТ", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Обработчик для клика по давлению
        private void BloodPressureText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string message = $"Ваше давление: {systolic}/{diastolic}\n";
            message += $"Пульс: {heartRate} уд/мин\n\n";
            message += $"Статус: {GetPressureDescription()}\n\n";
            message += "РЕКОМЕНДАЦИИ:\n";

            if (systolic < 90)
                message += "• Пейте больше воды\n• Ешьте чаще, но меньше\n• Выпейте кофе или чай";
            else if (systolic < 120)
                message += "• Отличное давление!\n• Продолжайте в том же духе";
            else if (systolic < 130)
                message += "• Нормальное давление\n• Следите за питанием";
            else if (systolic < 140)
                message += "• Уменьшите соль\n• Больше двигайтесь\n• Контролируйте вес";
            else if (systolic < 160)
                message += "• Обратитесь к врачу\n• Исключите соль\n• Откажитесь от алкоголя";
            else
                message += "• СРОЧНО к врачу!\n• Вызовите скорую при ухудшении";

            MessageBox.Show(message, "Анализ давления", MessageBoxButton.OK,
                          systolic >= 140 ? MessageBoxImage.Warning : MessageBoxImage.Information);
        }

        // Обработчики кнопок
        private void EditWeight_Click(object sender, RoutedEventArgs e)
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox("Введите новый вес (кг):", "Изменение веса", weight.ToString("F1"));
            if (double.TryParse(input, out double newWeight) && newWeight > 20 && newWeight < 300)
            {
                weight = newWeight;
                UpdateUI();
                ShowNotification("⚖️ Вес обновлен!");
            }
        }

        private void EditHeight_Click(object sender, RoutedEventArgs e)
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox("Введите новый рост (см):", "Изменение роста", height.ToString());
            if (double.TryParse(input, out double newHeight) && newHeight > 100 && newHeight < 250)
            {
                height = newHeight;
                UpdateUI();
                ShowNotification("📏 Рост обновлен!");
            }
        }

        private void AddVitals_Click(object sender, RoutedEventArgs e)
        {
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

                // ЛОКАЛЬНАЯ ВЕРСИЯ - БЕЗ API
                syncCoins += 5;
                UpdateUI();

                string status = GetPressureDescription();
                string recommendation = "";

                if (systolic >= 140)
                    recommendation = "\n\n⚠️ Внимание! Повышенное давление. Рекомендуется обратиться к врачу.";
                else if (systolic < 90)
                    recommendation = "\n\nПониженное давление. Пейте больше воды.";

                ShowNotification($"❤️ Показатели сохранены!\nДавление: {systolic}/{diastolic} - {status}{recommendation}\n+5 SyncCoin");
            }
        }

        private void LogSleep_Click(object sender, RoutedEventArgs e)
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox("Сколько часов вы спали?", "Сон", sleep.ToString("F1"));
            if (double.TryParse(input, out double newSleep) && newSleep > 0 && newSleep < 24)
            {
                sleep = newSleep;

                // ЛОКАЛЬНАЯ ВЕРСИЯ - БЕЗ API
                syncCoins += 3;
                UpdateUI();

                string description = GetSleepDescription();
                string recommendation = "";

                if (sleep < 6)
                    recommendation = "\n\n😴 Старайтесь спать больше - это важно для здоровья!";
                else if (sleep > 9)
                    recommendation = "\n\nВозможно, вы слишком много спите. Попробуйте просыпаться раньше.";

                ShowNotification($"😴 Сон записан: {sleep} ч - {description}{recommendation}\n+3 SyncCoin");
            }
        }

        // Кнопки быстрого ввода (верхние) - ЛОКАЛЬНАЯ ВЕРСИЯ
        private async void QuickSteps_Click(object sender, RoutedEventArgs e)
        {
            if (isStepsClicked) return;
            isStepsClicked = true;

            // ЛОКАЛЬНАЯ ВЕРСИЯ - БЕЗ API
            steps = Math.Min(stepsGoal, steps + 500);
            calories += 30;
            syncCoins += 1;

            UpdateUI();

            int remaining = stepsGoal - steps;
            if (remaining <= 0)
                ShowNotification("🎉 Поздравляем! Вы выполнили норму шагов!");
            else
                ShowNotification($"👣 +500 шагов! Осталось {remaining} шагов\n+1 SyncCoin");

            await Task.Delay(500);
            isStepsClicked = false;
        }

        private async void QuickWater_Click(object sender, RoutedEventArgs e)
        {
            if (isWaterClicked) return;
            isWaterClicked = true;

            // ЛОКАЛЬНАЯ ВЕРСИЯ - БЕЗ API
            water = Math.Min(waterGoal, water + 0.25);
            syncCoins += 1;

            UpdateUI();

            double remaining = waterGoal - water;
            if (remaining <= 0)
                ShowNotification("💧 Отлично! Вы выполнили норму воды!");
            else
                ShowNotification($"💧 +250 мл воды! Осталось {remaining:F1} л\n+1 SyncCoin");

            await Task.Delay(500);
            isWaterClicked = false;
        }

        private void QuickPressure_Click(object sender, RoutedEventArgs e)
        {
            // ЛОКАЛЬНАЯ ВЕРСИЯ - БЕЗ API
            systolic = random.Next(110, 125);
            diastolic = random.Next(70, 80);
            heartRate = random.Next(60, 75);
            syncCoins += 2;
            UpdateUI();

            string status = GetPressureDescription();
            ShowNotification($"🫀 Давление: {systolic}/{diastolic} - {status}\n+2 SyncCoin");
        }

        // Кнопки в целях (нижние) - ЛОКАЛЬНАЯ ВЕРСИЯ
        private async void AddSteps_Click(object sender, RoutedEventArgs e)
        {
            if (isStepsClicked) return;
            isStepsClicked = true;

            // ЛОКАЛЬНАЯ ВЕРСИЯ - БЕЗ API
            steps = Math.Min(stepsGoal, steps + 500);
            calories += 30;
            UpdateUI();

            await Task.Delay(500);
            isStepsClicked = false;
        }

        private async void AddWater_Click(object sender, RoutedEventArgs e)
        {
            if (isWaterClicked) return;
            isWaterClicked = true;

            // ЛОКАЛЬНАЯ ВЕРСИЯ - БЕЗ API
            water = Math.Min(waterGoal, water + 0.25);
            UpdateUI();

            await Task.Delay(500);
            isWaterClicked = false;
        }

        private void AddSleep_Click(object sender, RoutedEventArgs e)
        {
            // ЛОКАЛЬНАЯ ВЕРСИЯ - БЕЗ API
            sleep = Math.Min(sleepGoal, sleep + 1);
            UpdateUI();
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