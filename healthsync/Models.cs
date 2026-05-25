using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HealthSync
{
    public class DailyDataModel : INotifyPropertyChanged
    {
        private int _id;
        private DateTime _date;
        private int _steps;
        private int _waterMl;
        private double _sleepHours;
        private int _heartRate;
        private int _systolic;
        private int _diastolic;
        private string _mood;
        private double _energyLevel;
        private string _syncId;
        private bool _isSynced;

        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        public DateTime Date
        {
            get => _date;
            set { _date = value; OnPropertyChanged(); }
        }

        public int Steps
        {
            get => _steps;
            set { _steps = value; OnPropertyChanged(); OnPropertyChanged(nameof(StepsFormatted)); }
        }

        public string StepsFormatted => $"{Steps:N0}";

        public int WaterMl
        {
            get => _waterMl;
            set { _waterMl = value; OnPropertyChanged(); OnPropertyChanged(nameof(WaterLiters)); }
        }

        public double WaterLiters => Math.Round(WaterMl / 1000.0, 1);

        public double SleepHours
        {
            get => _sleepHours;
            set { _sleepHours = value; OnPropertyChanged(); }
        }

        public int HeartRate
        {
            get => _heartRate;
            set { _heartRate = value; OnPropertyChanged(); }
        }

        public int Systolic
        {
            get => _systolic;
            set { _systolic = value; OnPropertyChanged(); }
        }

        public int Diastolic
        {
            get => _diastolic;
            set { _diastolic = value; OnPropertyChanged(); }
        }

        public string Mood
        {
            get => _mood;
            set { _mood = value; OnPropertyChanged(); }
        }

        public double EnergyLevel
        {
            get => _energyLevel;
            set { _energyLevel = value; OnPropertyChanged(); }
        }

        public string SyncId
        {
            get => _syncId ?? Guid.NewGuid().ToString();
            set { _syncId = value; OnPropertyChanged(); }
        }

        public bool IsSynced
        {
            get => _isSynced;
            set { _isSynced = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class UserModel : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public double Height { get; set; } = 170;
        public double Weight { get; set; } = 70;
        public int Age { get; set; }
        public string Gender { get; set; }
        public int SyncCoins { get; set; } = 100;
        public string Theme { get; set; } = "light";
        public string City { get; set; } = "Moscow";

        // Цели
        public int StepsGoal { get; set; } = 10000;
        public double WaterGoal { get; set; } = 2.5;
        public double SleepGoal { get; set; } = 8.0;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}