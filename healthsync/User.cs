using System;
using System.Collections.Generic;

namespace HealthSync
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public double Height { get; set; }
        public double Weight { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
        public DateTime RegistrationDate { get; set; }
        public int SyncCoins { get; set; }

        // Показатели здоровья
        public int HeartRate { get; set; } = 68;
        public int Systolic { get; set; } = 118;
        public int Diastolic { get; set; } = 75;
        public int Steps { get; set; } = 0;
        public double Water { get; set; } = 0;
        public double Sleep { get; set; } = 0;
        public int Calories { get; set; } = 0;

        // История метрик (последние 7 дней)
        public List<int> StepsHistory { get; set; } = new List<int> { 0, 0, 0, 0, 0, 0, 0 };
        public List<double> WaterHistory { get; set; } = new List<double> { 0, 0, 0, 0, 0, 0, 0 };
        public List<double> SleepHistory { get; set; } = new List<double> { 0, 0, 0, 0, 0, 0, 0 };
        public List<int> CaloriesHistory { get; set; } = new List<int> { 0, 0, 0, 0, 0, 0, 0 };
        public List<int> HeartRateHistory { get; set; } = new List<int> { 68, 70, 72, 68, 71, 69, 68 };

        // Настройки
        public int StepsGoal { get; set; } = 10000;
        public double WaterGoal { get; set; } = 2.5;
        public double SleepGoal { get; set; } = 8.0;
        public int CaloriesGoal { get; set; } = 2000;
        public bool NotificationsEnabled { get; set; } = true;
        public string Theme { get; set; } = "Light";
        public bool AutoSync { get; set; } = true;
        public string UnitsSystem { get; set; } = "Metric";
        public bool DailyReminder { get; set; } = true;
        public string ReminderTime { get; set; } = "20:00";

      
        public bool StepsGoalAchieved { get; set; } = false;
        public bool WaterGoalAchieved { get; set; } = false;
        public bool SleepGoalAchieved { get; set; } = false;
        public bool CaloriesGoalAchieved { get; set; } = false;

        public string City { get; set; } = "Moscow";

        public DateTime LastUpdateDate { get; set; } = DateTime.Now.Date;
    }
}