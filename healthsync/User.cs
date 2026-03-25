using System;

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

        // Настройки
        public int StepsGoal { get; set; } = 10000;
        public double WaterGoal { get; set; } = 2.5;
        public double SleepGoal { get; set; } = 8.0;
        public bool NotificationsEnabled { get; set; } = true;
        public string Theme { get; set; } = "Light";
        public bool AutoSync { get; set; } = true;
        public string UnitsSystem { get; set; } = "Metric";
        public bool DailyReminder { get; set; } = true;
        public string ReminderTime { get; set; } = "20:00";
    }
}