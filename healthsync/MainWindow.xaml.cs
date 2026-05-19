using System;
using System.Collections.Generic;
using System.Linq;
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
        public double height = 178;
        private int userAge = 0;

        // Пульс и давление
        public int heartRate = 68;
        public int systolic = 118;
        public int diastolic = 75;

        // Дневные метрики
        public int steps = 0;
        public int stepsGoal = 10000;
        public double water = 0;
        public double waterGoal = 2.5;
        public double sleep = 0;
        public double sleepGoal = 8.0;
        public int calories = 0;
        public int caloriesGoal = 2000;

        private Random random = new Random();
        private DateTime lastUpdateDate = DateTime.Now.Date;

        // Переменные для отката
        private int lastSteps = 0;
        private double lastWater = 0;
        private double lastSleep = 0;
        private int lastCalories = 0;

        // Флаги
        private bool isRewarding = false;
        private bool isSetStepsClicked = false;
        private bool isSetWaterClicked = false;
        private bool isSetSleepClicked = false;
        private bool isSetCaloriesClicked = false;
        private bool isUndoStepsClicked = false;
        private bool isUndoWaterClicked = false;
        private bool isUndoSleepClicked = false;
        private bool isUndoCaloriesClicked = false;
        private bool isLogSleepClicked = false;
        private bool isAddVitalsClicked = false;
        private bool isEditWeightClicked = false;
        private bool isEditHeightClicked = false;
        private bool isWaterGoalClicked = false;
        private bool isStepsGoalClicked = false;
        private bool isSleepGoalClicked = false;
        private bool isCaloriesGoalClicked = false;

        // Текущая выбранная метрика для графика
        private string currentMetric = "Steps";

        // Погода
        private WeatherService weatherService = new WeatherService();

        public MainWindow()
        {
            InitializeComponent();
            Instance = this;

            UserManager.CreateDefaultUserIfNeeded();

            MainContent.Visibility = Visibility.Collapsed;
            MainFrame.Visibility = Visibility.Visible;
            MainFrame.Navigate(new LoginPage());

            InitializeData();
        }

        private void InitializeData()
        {
            CurrentDateText.Text = DateTime.Now.ToString("d MMMM yyyy");

            EditWeightButton.Click += EditWeight_Click;
            EditHeightButton.Click += EditHeight_Click;
            AddVitalsButton.Click += AddVitals_Click;
            LogSleepButton.Click += LogSleep_Click;

            AddWaterButton.Click += AddWater_Click;
            AddStepsButton.Click += AddSteps_Click;
            AddSleepButton.Click += AddSleep_Click;
            AddCaloriesButton.Click += AddCalories_Click;

            SetWaterButton.Click += SetWater_Click;
            SetStepsButton.Click += SetSteps_Click;
            SetSleepButton.Click += SetSleep_Click;
            SetCaloriesButton.Click += SetCalories_Click;

            UndoWaterButton.Click += UndoWater_Click;
            UndoStepsButton.Click += UndoSteps_Click;
            UndoSleepButton.Click += UndoSleep_Click;
            UndoCaloriesButton.Click += UndoCalories_Click;

            WaterGoalText.MouseDown += WaterGoalText_MouseDown;
            StepsGoalText.MouseDown += StepsGoalText_MouseDown;
            SleepGoalText.MouseDown += SleepGoalText_MouseDown;
            CaloriesGoalText.MouseDown += CaloriesGoalText_MouseDown;

            BMIText.MouseDown += BMIText_MouseDown;
            BloodPressureText.MouseDown += BloodPressureText_MouseDown;

            StepsRadio.Checked += MetricRadio_Checked;
            WaterRadio.Checked += MetricRadio_Checked;
            SleepRadio.Checked += MetricRadio_Checked;
            CaloriesRadio.Checked += MetricRadio_Checked;

            // Обновление погоды по кнопке
            RefreshWeatherButton.Click += RefreshWeather_Click;
        }

        public void LoadUserData()
        {
            if (CurrentUser != null)
            {
                userAge = CurrentUser.Age;

                stepsGoal = CurrentUser.StepsGoal;
                waterGoal = CurrentUser.WaterGoal;
                sleepGoal = CurrentUser.SleepGoal;
                caloriesGoal = CurrentUser.CaloriesGoal;
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

                lastUpdateDate = CurrentUser.LastUpdateDate == DateTime.MinValue ? DateTime.Now.Date : CurrentUser.LastUpdateDate.Date;

                if (lastUpdateDate != DateTime.Now.Date)
                {
                    SaveToHistory();
                    steps = 0;
                    water = 0;
                    sleep = 0;
                    calories = 0;
                    CurrentUser.Steps = 0;
                    CurrentUser.Water = 0;
                    CurrentUser.Sleep = 0;
                    CurrentUser.Calories = 0;

                    CurrentUser.StepsGoalAchieved = false;
                    CurrentUser.WaterGoalAchieved = false;
                    CurrentUser.SleepGoalAchieved = false;
                    CurrentUser.CaloriesGoalAchieved = false;

                    lastUpdateDate = DateTime.Now.Date;
                    CurrentUser.LastUpdateDate = lastUpdateDate;
                }

                if (CurrentUser.StepsHistory == null || CurrentUser.StepsHistory.Count == 0)
                {
                    CurrentUser.StepsHistory = new List<int>();
                    CurrentUser.WaterHistory = new List<double>();
                    CurrentUser.SleepHistory = new List<double>();
                    CurrentUser.CaloriesHistory = new List<int>();

                    for (int i = 0; i < 7; i++)
                    {
                        CurrentUser.StepsHistory.Add(0);
                        CurrentUser.WaterHistory.Add(0);
                        CurrentUser.SleepHistory.Add(0);
                        CurrentUser.CaloriesHistory.Add(0);
                    }
                }

                if (CurrentUser.StepsHistory.Count > 0)
                {
                    CurrentUser.StepsHistory[CurrentUser.StepsHistory.Count - 1] = steps;
                    CurrentUser.WaterHistory[CurrentUser.WaterHistory.Count - 1] = water;
                    CurrentUser.SleepHistory[CurrentUser.SleepHistory.Count - 1] = sleep;
                    CurrentUser.CaloriesHistory[CurrentUser.CaloriesHistory.Count - 1] = calories;
                }

                UpdateUI();
                LoadHistory();
                UpdateMetricsGraph();
                ApplyTheme(CurrentUser.Theme);

                // Загружаем погоду
                LoadWeather();
            }
        }

        public void UpdateUI()
        {
            if (CurrentUser != null)
            {
                stepsGoal = CurrentUser.StepsGoal;
                waterGoal = CurrentUser.WaterGoal;
                sleepGoal = CurrentUser.SleepGoal;
                caloriesGoal = CurrentUser.CaloriesGoal;
                syncCoins = CurrentUser.SyncCoins;
                height = CurrentUser.Height;
                weight = CurrentUser.Weight;
            }

            HeaderSyncCoinText.Text = syncCoins.ToString();
            WeightText.Text = weight.ToString("F1");
            HeightText.Text = height.ToString();

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

            HeartRateText.Text = heartRate.ToString();
            BloodPressureText.Text = $"{systolic}/{diastolic}";
            UpdatePressureColor();

            SleepHoursText.Text = sleep.ToString("F1");
            UpdateSleepColor();

            WaterGoalText.Text = $"{water:F1}/{waterGoal} л";
            WaterProgressBar.Value = (water / waterGoal) * 100;

            StepsGoalText.Text = $"{steps:N0}/{stepsGoal:N0}";
            StepsProgressBar.Value = ((double)steps / stepsGoal) * 100;

            SleepGoalText.Text = $"{sleep:F1}/{sleepGoal} ч";
            SleepProgressBar.Value = (sleep / sleepGoal) * 100;

            CaloriesGoalText.Text = $"{calories}/{caloriesGoal}";
            CaloriesProgressBar.Value = ((double)calories / caloriesGoal) * 100;

            FooterStepsText.Text = steps.ToString("N0");
            FooterCaloriesText.Text = calories.ToString("N0");
            FooterWaterText.Text = water.ToString("F1") + " л";
            FooterSleepText.Text = sleep.ToString("F1") + " ч";

            UpdateDailyInsight();
            UpdateBioAge();
            UpdateMetricsGraph();
        }

        // Загрузка погоды
        public async void LoadWeather()
        {
            try
            {
                var weather = await weatherService.GetWeatherAsync(); // без параметра

                WeatherTemp.Text = $"{weather.Temperature:F0}°C";
                WeatherCondition.Text = weather.Condition;
                WeatherRecommendation.Text = weather.Recommendation;

                // Иконка
                if (weather.Temperature <= 0)
                    WeatherIcon.Text = "❄️";
                else if (weather.Temperature <= 10)
                    WeatherIcon.Text = "🌬️";
                else if (weather.Temperature <= 20)
                    WeatherIcon.Text = "🌤️";
                else if (weather.Temperature <= 30)
                    WeatherIcon.Text = "☀️";
                else
                    WeatherIcon.Text = "🔥";
            }
            catch (Exception ex)
            {
                WeatherTemp.Text = "--°C";
                WeatherCondition.Text = "Ошибка";
            }
        }

        private void RefreshWeather_Click(object sender, RoutedEventArgs e)
        {
            LoadWeather();
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
                "💧 Вам нужно выпить " + (waterGoal - water).ToString("F1") + " л воды.",
                "👣 Сегодня вы прошли " + steps.ToString("N0") + " шагов. Осталось " + (stepsGoal - steps).ToString("N0"),
                "😴 " + GetSleepDescription() + " (" + sleep.ToString("F1") + " ч)",
                "⚖️ Ваш ИМТ в норме. Так держать!",
                "🫀 " + GetPressureDescription() + " - " + systolic + "/" + diastolic,
                "🔥 Сегодня сожжено " + calories + " калорий из " + caloriesGoal
            };
            DailyInsightText.Text = "💡 " + insights[random.Next(insights.Length)];
        }

        private void UpdateBioAge()
        {
            int actualAge = userAge;
            if (actualAge <= 0) actualAge = 28;
            int bioAge = actualAge;

            double bmi = weight / ((height / 100) * (height / 100));
            if (bmi < 18.5 || bmi > 30)
                bioAge += 2;
            else if (bmi >= 22 && bmi <= 25)
                bioAge -= 1;

            if (steps >= 10000)
                bioAge -= 2;
            else if (steps >= 7000)
                bioAge -= 1;
            else if (steps < 3000)
                bioAge += 3;
            else if (steps < 5000)
                bioAge += 1;

            if (sleep >= 7 && sleep <= 8)
                bioAge -= 2;
            else if (sleep < 5 || sleep > 9)
                bioAge += 3;
            else if (sleep < 6)
                bioAge += 1;

            if (heartRate >= 60 && heartRate <= 70)
                bioAge -= 1;
            else if (heartRate > 80)
                bioAge += 2;
            else if (heartRate > 90)
                bioAge += 4;

            if (systolic >= 110 && systolic <= 120)
                bioAge -= 1;
            else if (systolic > 130)
                bioAge += 2;
            else if (systolic > 140)
                bioAge += 4;

            if (water >= 2.0)
                bioAge -= 1;
            else if (water < 1.0)
                bioAge += 1;

            bioAge = Math.Max(18, Math.Min(80, bioAge));

            string comparison;
            if (bioAge < actualAge)
                comparison = "🏆 Вы моложе своего возраста! Так держать!";
            else if (bioAge > actualAge)
                comparison = "⚠️ Ваш организм старше. Пора заняться здоровьем!";
            else
                comparison = "✅ Ваш биологический возраст соответствует календарному.";

            BioAgeText.Text = $"Биологический возраст: {bioAge}\n{comparison}";
        }

        private void LoadHistory()
        {
            var history = new List<HistoryItem>
            {
                new HistoryItem { Icon = "❤️", Title = "Пульс", Time = DateTime.Now.ToString("HH:mm"), Value = $"{heartRate} уд/мин", Color = "#F44336" },
                new HistoryItem { Icon = "🫀", Title = "Давление", Time = DateTime.Now.ToString("HH:mm"), Value = $"{systolic}/{diastolic}", Color = "#2196F3" },
                new HistoryItem { Icon = "⚖️", Title = "Вес", Time = "Сегодня", Value = $"{weight} кг", Color = "#4CAF50" },
                new HistoryItem { Icon = "😴", Title = "Сон", Time = "Сегодня", Value = $"{sleep} ч", Color = "#5C6BC0" },
                new HistoryItem { Icon = "🔥", Title = "Калории", Time = "Сегодня", Value = $"{calories} ккал", Color = "#FF5722" }
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
                CurrentUser.StepsGoal = stepsGoal;
                CurrentUser.WaterGoal = waterGoal;
                CurrentUser.SleepGoal = sleepGoal;
                CurrentUser.CaloriesGoal = caloriesGoal;
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
                CurrentUser.LastUpdateDate = lastUpdateDate;
                CurrentUser.Age = userAge;

                SaveToHistory();
                UserManager.UpdateUser(CurrentUser);
            }
        }

        private void SaveToHistory()
        {
            if (CurrentUser == null) return;
            DateTime today = DateTime.Now.Date;

            if (lastUpdateDate != today)
            {
                CurrentUser.StepsHistory.Add(steps);
                CurrentUser.WaterHistory.Add(water);
                CurrentUser.SleepHistory.Add(sleep);
                CurrentUser.CaloriesHistory.Add(calories);

                while (CurrentUser.StepsHistory.Count > 7) CurrentUser.StepsHistory.RemoveAt(0);
                while (CurrentUser.WaterHistory.Count > 7) CurrentUser.WaterHistory.RemoveAt(0);
                while (CurrentUser.SleepHistory.Count > 7) CurrentUser.SleepHistory.RemoveAt(0);
                while (CurrentUser.CaloriesHistory.Count > 7) CurrentUser.CaloriesHistory.RemoveAt(0);

                lastUpdateDate = today;
            }
            else
            {
                if (CurrentUser.StepsHistory.Count > 0)
                    CurrentUser.StepsHistory[CurrentUser.StepsHistory.Count - 1] = steps;
                if (CurrentUser.WaterHistory.Count > 0)
                    CurrentUser.WaterHistory[CurrentUser.WaterHistory.Count - 1] = water;
                if (CurrentUser.SleepHistory.Count > 0)
                    CurrentUser.SleepHistory[CurrentUser.SleepHistory.Count - 1] = sleep;
                if (CurrentUser.CaloriesHistory.Count > 0)
                    CurrentUser.CaloriesHistory[CurrentUser.CaloriesHistory.Count - 1] = calories;
            }
        }

        private void CheckGoalsAndReward()
        {
            if (isRewarding) return;
            isRewarding = true;

            try
            {
                if (CurrentUser == null) return;

                bool goalAchieved = false;
                string rewards = "";

                if (steps >= stepsGoal && steps > 0)
                {
                    if (!CurrentUser.StepsGoalAchieved)
                    {
                        CurrentUser.StepsGoalAchieved = true;
                        syncCoins += 20;
                        CurrentUser.SyncCoins = syncCoins;
                        rewards += "👣 +20 монет за выполнение цели по шагам!\n";
                        goalAchieved = true;
                    }
                }

                if (water >= waterGoal && water > 0)
                {
                    if (!CurrentUser.WaterGoalAchieved)
                    {
                        CurrentUser.WaterGoalAchieved = true;
                        syncCoins += 15;
                        CurrentUser.SyncCoins = syncCoins;
                        rewards += "💧 +15 монет за выполнение цели по воде!\n";
                        goalAchieved = true;
                    }
                }

                if (sleep >= sleepGoal && sleep > 0)
                {
                    if (!CurrentUser.SleepGoalAchieved)
                    {
                        CurrentUser.SleepGoalAchieved = true;
                        syncCoins += 25;
                        CurrentUser.SyncCoins = syncCoins;
                        rewards += "😴 +25 монет за выполнение цели по сну!\n";
                        goalAchieved = true;
                    }
                }

                if (calories >= caloriesGoal && calories > 0)
                {
                    if (!CurrentUser.CaloriesGoalAchieved)
                    {
                        CurrentUser.CaloriesGoalAchieved = true;
                        syncCoins += 10;
                        CurrentUser.SyncCoins = syncCoins;
                        rewards += "🔥 +10 монет за выполнение цели по калориям!\n";
                        goalAchieved = true;
                    }
                }

                if (goalAchieved)
                {
                    ShowNotification($"🎉 Поздравляем! Вы получили:\n{rewards}", "Награда!");
                    AnimateCoin();
                    UpdateUI();
                }
            }
            finally
            {
                isRewarding = false;
            }
        }

        private async void AnimateCoin()
        {
            var coinText = HeaderSyncCoinText;
            var originalFontSize = coinText.FontSize;

            coinText.FontSize = originalFontSize + 4;
            coinText.Foreground = new SolidColorBrush(Color.FromRgb(255, 215, 0));

            await Task.Delay(200);

            coinText.FontSize = originalFontSize;
            coinText.Foreground = Brushes.White;
        }

        private void UpdateMetricsGraph()
        {
            if (CurrentUser == null || MetricsGraphGrid == null) return;

            double[] values = GetLast7DaysValues();
            double targetValue = 0;
            string unit = "";
            SolidColorBrush color = new SolidColorBrush(Color.FromRgb(76, 175, 80));

            switch (currentMetric)
            {
                case "Steps":
                    targetValue = stepsGoal;
                    unit = "шагов";
                    color = new SolidColorBrush(Color.FromRgb(76, 175, 80));
                    CurrentMetricValue.Text = $"Шаги за неделю: {values.Sum():N0} / {stepsGoal * 7:N0}";
                    CurrentMetricValue.Foreground = color;
                    break;
                case "Water":
                    targetValue = waterGoal;
                    unit = "л";
                    color = new SolidColorBrush(Color.FromRgb(33, 150, 243));
                    CurrentMetricValue.Text = $"Вода за неделю: {values.Sum():F1} / {waterGoal * 7:F1} л";
                    CurrentMetricValue.Foreground = color;
                    break;
                case "Sleep":
                    targetValue = sleepGoal;
                    unit = "ч";
                    color = new SolidColorBrush(Color.FromRgb(92, 107, 192));
                    CurrentMetricValue.Text = $"Сон за неделю: {values.Sum():F1} / {sleepGoal * 7:F1} ч";
                    CurrentMetricValue.Foreground = color;
                    break;
                case "Calories":
                    targetValue = caloriesGoal;
                    unit = "ккал";
                    color = new SolidColorBrush(Color.FromRgb(255, 87, 34));
                    CurrentMetricValue.Text = $"Калории за неделю: {values.Sum():N0} / {caloriesGoal * 7:N0} ккал";
                    CurrentMetricValue.Foreground = color;
                    break;
            }

            double maxWeeklyValue = values.Max();
            double maxGraphValue = Math.Max(targetValue, maxWeeklyValue);
            if (maxGraphValue <= 0) maxGraphValue = 1;

            int todayIndex = ((int)DateTime.Now.DayOfWeek + 6) % 7;

            for (int i = 0; i < Math.Min(values.Length, 7) && i < MetricsGraphGrid.Children.Count; i++)
            {
                var border = MetricsGraphGrid.Children[i] as Border;
                if (border != null)
                {
                    double heightPercent = (values[i] / maxGraphValue) * 100;
                    double height = Math.Max(15, Math.Min(140, heightPercent));
                    border.Height = height;

                    if (values[i] >= targetValue && values[i] > 0)
                    {
                        border.Background = new SolidColorBrush(Color.FromRgb(255, 215, 0));
                    }
                    else
                    {
                        border.Background = color;
                    }

                    border.ToolTip = $"{values[i]:F1} {unit} (цель: {targetValue:F0})";

                    if (i == todayIndex && values[i] > 0)
                    {
                        border.BorderBrush = Brushes.Gold;
                        border.BorderThickness = new Thickness(2);
                    }
                    else
                    {
                        border.BorderThickness = new Thickness(0);
                    }
                }
            }

            UpdateTargetLine(targetValue, maxGraphValue);
        }

        private double[] GetLast7DaysValues()
        {
            double[] result = new double[7];
            DateTime today = DateTime.Now.Date;

            DateTime startOfWeek = today.AddDays(-(int)today.DayOfWeek + 1);
            if (today.DayOfWeek == DayOfWeek.Sunday)
                startOfWeek = today.AddDays(-6);

            for (int i = 0; i < 7; i++)
            {
                DateTime day = startOfWeek.AddDays(i);
                double value = 0;

                switch (currentMetric)
                {
                    case "Steps":
                        value = GetStepsForDate(day);
                        break;
                    case "Water":
                        value = GetWaterForDate(day);
                        break;
                    case "Sleep":
                        value = GetSleepForDate(day);
                        break;
                    case "Calories":
                        value = GetCaloriesForDate(day);
                        break;
                }
                result[i] = value;
            }
            return result;
        }

        private int GetStepsForDate(DateTime date)
        {
            if (CurrentUser?.StepsHistory != null)
            {
                int dayIndex = (date - DateTime.Now.Date).Days;
                if (dayIndex >= -6 && dayIndex <= 0)
                {
                    int historyIndex = 6 + dayIndex;
                    if (historyIndex >= 0 && historyIndex < CurrentUser.StepsHistory.Count)
                    {
                        return CurrentUser.StepsHistory[historyIndex];
                    }
                }
            }
            return date.Date == DateTime.Now.Date ? steps : 0;
        }

        private double GetWaterForDate(DateTime date)
        {
            if (CurrentUser?.WaterHistory != null)
            {
                int dayIndex = (date - DateTime.Now.Date).Days;
                if (dayIndex >= -6 && dayIndex <= 0)
                {
                    int historyIndex = 6 + dayIndex;
                    if (historyIndex >= 0 && historyIndex < CurrentUser.WaterHistory.Count)
                    {
                        return CurrentUser.WaterHistory[historyIndex];
                    }
                }
            }
            return date.Date == DateTime.Now.Date ? water : 0;
        }

        private double GetSleepForDate(DateTime date)
        {
            if (CurrentUser?.SleepHistory != null)
            {
                int dayIndex = (date - DateTime.Now.Date).Days;
                if (dayIndex >= -6 && dayIndex <= 0)
                {
                    int historyIndex = 6 + dayIndex;
                    if (historyIndex >= 0 && historyIndex < CurrentUser.SleepHistory.Count)
                    {
                        return CurrentUser.SleepHistory[historyIndex];
                    }
                }
            }
            return date.Date == DateTime.Now.Date ? sleep : 0;
        }

        private int GetCaloriesForDate(DateTime date)
        {
            if (CurrentUser?.CaloriesHistory != null)
            {
                int dayIndex = (date - DateTime.Now.Date).Days;
                if (dayIndex >= -6 && dayIndex <= 0)
                {
                    int historyIndex = 6 + dayIndex;
                    if (historyIndex >= 0 && historyIndex < CurrentUser.CaloriesHistory.Count)
                    {
                        return CurrentUser.CaloriesHistory[historyIndex];
                    }
                }
            }
            return date.Date == DateTime.Now.Date ? calories : 0;
        }

        private void UpdateTargetLine(double targetValue, double maxGraphValue)
        {
            if (MetricsGraphGrid == null || TargetLine == null) return;

            string unit = "";

            switch (currentMetric)
            {
                case "Steps": unit = "шагов"; break;
                case "Water": unit = "л"; break;
                case "Sleep": unit = "ч"; break;
                case "Calories": unit = "ккал"; break;
            }

            double targetHeightPercent = (targetValue / maxGraphValue) * 100;
            double targetHeight = Math.Max(0, Math.Min(140, targetHeightPercent));

            TargetLine.Margin = new Thickness(0, 0, 0, targetHeight);
            TargetMarker.Margin = new Thickness(0, 0, 10, targetHeight - 5);
            TargetText.Margin = new Thickness(0, 0, 25, targetHeight - 8);
            TargetText.Text = $"🎯 {targetValue:F0} {unit}";

            SolidColorBrush color;
            switch (currentMetric)
            {
                case "Steps": color = new SolidColorBrush(Color.FromRgb(76, 175, 80)); break;
                case "Water": color = new SolidColorBrush(Color.FromRgb(33, 150, 243)); break;
                case "Sleep": color = new SolidColorBrush(Color.FromRgb(92, 107, 192)); break;
                case "Calories": color = new SolidColorBrush(Color.FromRgb(255, 87, 34)); break;
                default: color = new SolidColorBrush(Color.FromRgb(76, 175, 80)); break;
            }

            TargetLine.Background = color;
            TargetMarker.Fill = color;
            TargetText.Foreground = color;
        }

        private void MetricRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == StepsRadio)
                currentMetric = "Steps";
            else if (sender == WaterRadio)
                currentMetric = "Water";
            else if (sender == SleepRadio)
                currentMetric = "Sleep";
            else if (sender == CaloriesRadio)
                currentMetric = "Calories";

            UpdateMetricsGraph();
        }

        // === НАСТРОЙКА ЦЕЛЕЙ ===
        private async void WaterGoalText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isWaterGoalClicked) return;
            isWaterGoalClicked = true;
            string input = Microsoft.VisualBasic.Interaction.InputBox("Введите цель по воде (литры):", "Настройка цели", waterGoal.ToString("F1"));
            if (double.TryParse(input, out double newGoal) && newGoal > 0 && newGoal < 10)
            {
                waterGoal = newGoal;
                if (CurrentUser != null) CurrentUser.WaterGoal = newGoal;
                UpdateUI();
                LoadHistory();
                ShowNotification($"💧 Цель по воде: {waterGoal:F1} л", "Успешно");
            }
            await Task.Delay(500);
            isWaterGoalClicked = false;
        }

        private async void StepsGoalText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isStepsGoalClicked) return;
            isStepsGoalClicked = true;
            string input = Microsoft.VisualBasic.Interaction.InputBox("Введите цель по шагам:", "Настройка цели", stepsGoal.ToString());
            if (int.TryParse(input, out int newGoal) && newGoal > 0 && newGoal < 50000)
            {
                stepsGoal = newGoal;
                if (CurrentUser != null) CurrentUser.StepsGoal = newGoal;
                UpdateUI();
                LoadHistory();
                ShowNotification($"👣 Цель по шагам: {stepsGoal:N0}", "Успешно");
            }
            await Task.Delay(500);
            isStepsGoalClicked = false;
        }

        private async void SleepGoalText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isSleepGoalClicked) return;
            isSleepGoalClicked = true;
            string input = Microsoft.VisualBasic.Interaction.InputBox("Введите цель по сну (часы):", "Настройка цели", sleepGoal.ToString("F1"));
            if (double.TryParse(input, out double newGoal) && newGoal > 0 && newGoal < 24)
            {
                sleepGoal = newGoal;
                if (CurrentUser != null) CurrentUser.SleepGoal = newGoal;
                UpdateUI();
                LoadHistory();
                ShowNotification($"😴 Цель по сну: {sleepGoal:F1} ч", "Успешно");
            }
            await Task.Delay(500);
            isSleepGoalClicked = false;
        }

        private async void CaloriesGoalText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isCaloriesGoalClicked) return;
            isCaloriesGoalClicked = true;
            string input = Microsoft.VisualBasic.Interaction.InputBox("Введите цель по калориям:", "Настройка цели", caloriesGoal.ToString());
            if (int.TryParse(input, out int newGoal) && newGoal > 0 && newGoal < 10000)
            {
                caloriesGoal = newGoal;
                if (CurrentUser != null) CurrentUser.CaloriesGoal = newGoal;
                UpdateUI();
                LoadHistory();
                ShowNotification($"🔥 Цель по калориям: {caloriesGoal} ккал", "Успешно");
            }
            await Task.Delay(500);
            isCaloriesGoalClicked = false;
        }

        // === АНАЛИЗ ===
        private async void BMIText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            double bmi = weight / ((height / 100) * (height / 100));
            string message = $"Ваш ИМТ: {bmi:F1}\n\nКатегория: {BMICategoryText.Text}\n\nРЕКОМЕНДАЦИИ:\n";
            if (bmi < 18.5)
                message += "• Увеличьте калорийность рациона\n• Добавьте белки и полезные жиры";
            else if (bmi < 25)
                message += "• Отличный показатель!\n• Продолжайте в том же духе";
            else if (bmi < 30)
                message += "• Увеличьте физическую активность\n• Уменьшите потребление сахара";
            else
                message += "• Обратитесь к врачу\n• Разработайте план снижения веса";
            MessageBox.Show(message, "Анализ ИМТ", MessageBoxButton.OK, MessageBoxImage.Information);
            await Task.Delay(500);
        }

        private async void BloodPressureText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string message = $"Давление: {systolic}/{diastolic}\nПульс: {heartRate} уд/мин\n\nСтатус: {GetPressureDescription()}\n\nРЕКОМЕНДАЦИИ:\n";
            if (systolic < 90)
                message += "• Пейте больше воды\n• Ешьте чаще, но меньше";
            else if (systolic < 120)
                message += "• Отличное давление!\n• Продолжайте в том же духе";
            else if (systolic < 130)
                message += "• Нормальное давление\n• Следите за питанием";
            else if (systolic < 140)
                message += "• Уменьшите соль\n• Больше двигайтесь";
            else if (systolic < 160)
                message += "• Обратитесь к врачу\n• Исключите соль";
            else
                message += "• СРОЧНО к врачу!\n• Вызовите скорую";
            MessageBox.Show(message, "Анализ давления", MessageBoxButton.OK, MessageBoxImage.Information);
            await Task.Delay(500);
        }

        // === РЕДАКТИРОВАНИЕ ВЕСА И РОСТА ===
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
                LoadHistory();
                ShowNotification($"⚖️ Вес: {weight:F1} кг", "Успешно");
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
                LoadHistory();
                ShowNotification($"📏 Рост: {height} см", "Успешно");
            }
            await Task.Delay(500);
            isEditHeightClicked = false;
        }

        // === ДОБАВЛЕНИЕ ПОКАЗАТЕЛЕЙ ===
        private async void AddVitals_Click(object sender, RoutedEventArgs e)
        {
            if (isAddVitalsClicked) return;
            isAddVitalsClicked = true;
            string pulseInput = Microsoft.VisualBasic.Interaction.InputBox("Пульс (уд/мин):", "Пульс", heartRate.ToString());
            string bpInput = Microsoft.VisualBasic.Interaction.InputBox("Давление (сист/диаст):", "Давление", $"{systolic}/{diastolic}");
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
                LoadHistory();
                SaveToHistory();
                CheckGoalsAndReward();
                ShowNotification($"❤️ Показатели сохранены!\nДавление: {systolic}/{diastolic}\n+5 SyncCoin", "Успешно");
            }
            await Task.Delay(500);
            isAddVitalsClicked = false;
        }

        private async void LogSleep_Click(object sender, RoutedEventArgs e)
        {
            if (isLogSleepClicked) return;
            isLogSleepClicked = true;
            string input = Microsoft.VisualBasic.Interaction.InputBox("Сколько часов спали?", "Сон", sleep.ToString("F1"));
            if (double.TryParse(input, out double newSleep) && newSleep > 0 && newSleep <= 24)
            {
                lastSleep = sleep;
                sleep = newSleep;
                syncCoins += 3;
                if (CurrentUser != null) CurrentUser.SyncCoins = syncCoins;

                if (CurrentUser != null && CurrentUser.SleepHistory.Count > 0)
                {
                    CurrentUser.SleepHistory[CurrentUser.SleepHistory.Count - 1] = sleep;
                }

                UpdateUI();
                LoadHistory();
                SaveToHistory();
                CheckGoalsAndReward();
                ShowNotification($"😴 Сон: {sleep} ч\n+3 SyncCoin", "Успешно");
            }
            await Task.Delay(500);
            isLogSleepClicked = false;
        }

        // === КНОПКИ "+" ===
        private async void AddSteps_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null || !btn.IsEnabled) return;

            btn.IsEnabled = false;

            try
            {
                lastSteps = steps;
                steps += 500;
                calories += 30;

                if (CurrentUser != null && CurrentUser.StepsHistory.Count > 0)
                {
                    CurrentUser.StepsHistory[CurrentUser.StepsHistory.Count - 1] = steps;
                }

                UpdateUI();
                LoadHistory();
                SaveToHistory();
                CheckGoalsAndReward();
            }
            finally
            {
                await Task.Delay(300);
                btn.IsEnabled = true;
            }
        }

        private async void AddWater_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null || !btn.IsEnabled) return;

            btn.IsEnabled = false;

            try
            {
                lastWater = water;
                water += 0.25;

                if (CurrentUser != null && CurrentUser.WaterHistory.Count > 0)
                {
                    CurrentUser.WaterHistory[CurrentUser.WaterHistory.Count - 1] = water;
                }

                UpdateUI();
                LoadHistory();
                SaveToHistory();
                CheckGoalsAndReward();
            }
            finally
            {
                await Task.Delay(300);
                btn.IsEnabled = true;
            }
        }

        private async void AddSleep_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null || !btn.IsEnabled) return;

            btn.IsEnabled = false;

            try
            {
                lastSleep = sleep;
                sleep += 1;

                if (CurrentUser != null && CurrentUser.SleepHistory.Count > 0)
                {
                    CurrentUser.SleepHistory[CurrentUser.SleepHistory.Count - 1] = sleep;
                }

                UpdateUI();
                LoadHistory();
                SaveToHistory();
                CheckGoalsAndReward();
            }
            finally
            {
                await Task.Delay(300);
                btn.IsEnabled = true;
            }
        }

        private async void AddCalories_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null || !btn.IsEnabled) return;

            btn.IsEnabled = false;

            try
            {
                lastCalories = calories;
                calories += 100;

                if (CurrentUser != null && CurrentUser.CaloriesHistory.Count > 0)
                {
                    CurrentUser.CaloriesHistory[CurrentUser.CaloriesHistory.Count - 1] = calories;
                }

                UpdateUI();
                LoadHistory();
                SaveToHistory();
                CheckGoalsAndReward();
            }
            finally
            {
                await Task.Delay(300);
                btn.IsEnabled = true;
            }
        }

        // === КНОПКИ "✏️" ===
        private async void SetSteps_Click(object sender, RoutedEventArgs e)
        {
            if (isSetStepsClicked) return;
            isSetStepsClicked = true;
            string input = Microsoft.VisualBasic.Interaction.InputBox("Введите количество шагов:", "Установить шаги", steps.ToString());
            if (int.TryParse(input, out int newValue) && newValue >= 0 && newValue <= 100000)
            {
                lastSteps = steps;
                steps = newValue;

                if (CurrentUser != null && CurrentUser.StepsHistory.Count > 0)
                {
                    CurrentUser.StepsHistory[CurrentUser.StepsHistory.Count - 1] = steps;
                }

                UpdateUI();
                LoadHistory();
                SaveToHistory();
                CheckGoalsAndReward();
            }
            else
            {
                ShowNotification("Введите корректное количество шагов (0-100000)", "Ошибка");
            }
            await Task.Delay(500);
            isSetStepsClicked = false;
        }

        private async void SetWater_Click(object sender, RoutedEventArgs e)
        {
            if (isSetWaterClicked) return;
            isSetWaterClicked = true;
            string input = Microsoft.VisualBasic.Interaction.InputBox("Введите количество воды (литры):", "Установить воду", water.ToString("F1"));
            if (double.TryParse(input, out double newValue) && newValue >= 0 && newValue <= 10)
            {
                lastWater = water;
                water = newValue;

                if (CurrentUser != null && CurrentUser.WaterHistory.Count > 0)
                {
                    CurrentUser.WaterHistory[CurrentUser.WaterHistory.Count - 1] = water;
                }

                UpdateUI();
                LoadHistory();
                SaveToHistory();
                CheckGoalsAndReward();
            }
            else
            {
                ShowNotification("Введите корректное количество воды (0-10 л)", "Ошибка");
            }
            await Task.Delay(500);
            isSetWaterClicked = false;
        }

        private async void SetSleep_Click(object sender, RoutedEventArgs e)
        {
            if (isSetSleepClicked) return;
            isSetSleepClicked = true;
            string input = Microsoft.VisualBasic.Interaction.InputBox("Введите количество часов сна:", "Установить сон", sleep.ToString("F1"));
            if (double.TryParse(input, out double newValue) && newValue >= 0 && newValue <= 24)
            {
                lastSleep = sleep;
                sleep = newValue;

                if (CurrentUser != null && CurrentUser.SleepHistory.Count > 0)
                {
                    CurrentUser.SleepHistory[CurrentUser.SleepHistory.Count - 1] = sleep;
                }

                UpdateUI();
                LoadHistory();
                SaveToHistory();
                CheckGoalsAndReward();
            }
            else
            {
                ShowNotification("Введите корректное количество часов (0-24)", "Ошибка");
            }
            await Task.Delay(500);
            isSetSleepClicked = false;
        }

        private async void SetCalories_Click(object sender, RoutedEventArgs e)
        {
            if (isSetCaloriesClicked) return;
            isSetCaloriesClicked = true;
            string input = Microsoft.VisualBasic.Interaction.InputBox("Введите количество калорий:", "Установить калории", calories.ToString());
            if (int.TryParse(input, out int newValue) && newValue >= 0 && newValue <= 10000)
            {
                lastCalories = calories;
                calories = newValue;

                if (CurrentUser != null && CurrentUser.CaloriesHistory.Count > 0)
                {
                    CurrentUser.CaloriesHistory[CurrentUser.CaloriesHistory.Count - 1] = calories;
                }

                UpdateUI();
                LoadHistory();
                SaveToHistory();
                CheckGoalsAndReward();
            }
            else
            {
                ShowNotification("Введите корректное количество калорий (0-10000)", "Ошибка");
            }
            await Task.Delay(500);
            isSetCaloriesClicked = false;
        }

        // === КНОПКИ "↩️" ===
        private async void UndoSteps_Click(object sender, RoutedEventArgs e)
        {
            if (isUndoStepsClicked) return;
            isUndoStepsClicked = true;

            if (lastSteps == 0 && steps == 0)
            {
                ShowNotification("Нет действий для отката", "Информация");
                isUndoStepsClicked = false;
                return;
            }

            steps = lastSteps;
            lastSteps = 0;

            if (CurrentUser != null && CurrentUser.StepsHistory.Count > 0)
            {
                CurrentUser.StepsHistory[CurrentUser.StepsHistory.Count - 1] = steps;
            }

            UpdateUI();
            LoadHistory();
            SaveToHistory();

            await Task.Delay(500);
            isUndoStepsClicked = false;
        }

        private async void UndoWater_Click(object sender, RoutedEventArgs e)
        {
            if (isUndoWaterClicked) return;
            isUndoWaterClicked = true;

            if (lastWater == 0 && water == 0)
            {
                ShowNotification("Нет действий для отката", "Информация");
                isUndoWaterClicked = false;
                return;
            }

            water = lastWater;
            lastWater = 0;

            if (CurrentUser != null && CurrentUser.WaterHistory.Count > 0)
            {
                CurrentUser.WaterHistory[CurrentUser.WaterHistory.Count - 1] = water;
            }

            UpdateUI();
            LoadHistory();
            SaveToHistory();

            await Task.Delay(500);
            isUndoWaterClicked = false;
        }

        private async void UndoSleep_Click(object sender, RoutedEventArgs e)
        {
            if (isUndoSleepClicked) return;
            isUndoSleepClicked = true;

            if (lastSleep == 0 && sleep == 0)
            {
                ShowNotification("Нет действий для отката", "Информация");
                isUndoSleepClicked = false;
                return;
            }

            sleep = lastSleep;
            lastSleep = 0;

            if (CurrentUser != null && CurrentUser.SleepHistory.Count > 0)
            {
                CurrentUser.SleepHistory[CurrentUser.SleepHistory.Count - 1] = sleep;
            }

            UpdateUI();
            LoadHistory();
            SaveToHistory();

            await Task.Delay(500);
            isUndoSleepClicked = false;
        }

        private async void UndoCalories_Click(object sender, RoutedEventArgs e)
        {
            if (isUndoCaloriesClicked) return;
            isUndoCaloriesClicked = true;

            if (lastCalories == 0 && calories == 0)
            {
                ShowNotification("Нет действий для отката", "Информация");
                isUndoCaloriesClicked = false;
                return;
            }

            calories = lastCalories;
            lastCalories = 0;

            if (CurrentUser != null && CurrentUser.CaloriesHistory.Count > 0)
            {
                CurrentUser.CaloriesHistory[CurrentUser.CaloriesHistory.Count - 1] = calories;
            }

            UpdateUI();
            LoadHistory();
            SaveToHistory();

            await Task.Delay(500);
            isUndoCaloriesClicked = false;
        }

        // === НАВИГАЦИЯ ===
        public void NavigateToPage(Page page)
        {
            MainContent.Visibility = Visibility.Collapsed;
            MainFrame.Visibility = Visibility.Visible;
            MainFrame.Navigate(page);
        }

        public void ShowMainContent()
        {
            SaveCurrentUser();
            MainFrame.Visibility = Visibility.Collapsed;
            MainContent.Visibility = Visibility.Visible;
            if (CurrentUser != null)
            {
                stepsGoal = CurrentUser.StepsGoal;
                waterGoal = CurrentUser.WaterGoal;
                sleepGoal = CurrentUser.SleepGoal;
                caloriesGoal = CurrentUser.CaloriesGoal;
                syncCoins = CurrentUser.SyncCoins;
                height = CurrentUser.Height;
                weight = CurrentUser.Weight;
            }
            UpdateUI();
            LoadHistory();
            UpdateMetricsGraph();
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUser != null) ShowMainContent();
            else NavigateToPage(new LoginPage());
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUser != null) NavigateToPage(new ProfilePage());
            else NavigateToPage(new LoginPage());
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUser != null) NavigateToPage(new SettingsPage());
            else NavigateToPage(new LoginPage());
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

    public class HistoryItem
    {
        public string Icon { get; set; }
        public string Title { get; set; }
        public string Time { get; set; }
        public string Value { get; set; }
        public string Color { get; set; }
    }
}