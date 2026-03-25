using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace HealthSync
{
    public static class UserManager
    {
        private static List<User> users = new List<User>();
        private static readonly string dataFile = "users.json";

        static UserManager()
        {
            LoadUsers();
        }

        private static void LoadUsers()
        {
            try
            {
                if (File.Exists(dataFile))
                {
                    string json = File.ReadAllText(dataFile);
                    users = JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
                }
            }
            catch
            {
                users = new List<User>();
            }
        }

        private static void SaveUsers()
        {
            try
            {
                string json = JsonSerializer.Serialize(users);
                File.WriteAllText(dataFile, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения: {ex.Message}");
            }
        }

        public static bool RegisterUser(string username, string email, string password, double height, double weight, int age, string gender)
        {
            if (users.Any(u => u.Username == username || u.Email == email))
                return false;

            var user = new User
            {
                Id = users.Count > 0 ? users.Max(u => u.Id) + 1 : 1,
                Username = username,
                Email = email,
                Password = password,
                Height = height,
                Weight = weight,
                Age = age,
                Gender = gender,
                RegistrationDate = DateTime.Now,
                SyncCoins = 100,
                StepsGoal = 10000,
                WaterGoal = 2.5,
                SleepGoal = 8.0,
                NotificationsEnabled = true,
                Theme = "Light",
                AutoSync = true,
                UnitsSystem = "Metric",
                DailyReminder = true,
                ReminderTime = "20:00"
            };

            users.Add(user);
            SaveUsers();
            return true;
        }

        public static User LoginUser(string username, string password)
        {
            return users.FirstOrDefault(u => (u.Username == username || u.Email == username) && u.Password == password);
        }

        public static void UpdateUser(User user)
        {
            var existing = users.FirstOrDefault(u => u.Id == user.Id);
            if (existing != null)
            {
                existing.Username = user.Username;
                existing.Email = user.Email;
                existing.Height = user.Height;
                existing.Weight = user.Weight;
                existing.Age = user.Age;
                existing.Gender = user.Gender;
                existing.StepsGoal = user.StepsGoal;
                existing.WaterGoal = user.WaterGoal;
                existing.SleepGoal = user.SleepGoal;
                existing.NotificationsEnabled = user.NotificationsEnabled;
                existing.Theme = user.Theme;
                existing.AutoSync = user.AutoSync;
                existing.UnitsSystem = user.UnitsSystem;
                existing.DailyReminder = user.DailyReminder;
                existing.ReminderTime = user.ReminderTime;
                existing.SyncCoins = user.SyncCoins;
                SaveUsers();
            }
        }

        public static bool ChangePassword(int userId, string oldPassword, string newPassword)
        {
            var user = users.FirstOrDefault(u => u.Id == userId);
            if (user != null && user.Password == oldPassword)
            {
                user.Password = newPassword;
                SaveUsers();
                return true;
            }
            return false;
        }

        public static User GetUserById(int id)
        {
            return users.FirstOrDefault(u => u.Id == id);
        }

        public static void CreateDefaultUserIfNeeded()
        {
            if (!users.Any())
            {
                var defaultUser = new User
                {
                    Id = 1,
                    Username = "Пользователь",
                    Email = "user@healthsync.com",
                    Password = "123",
                    Height = 170,
                    Weight = 70,
                    Age = 25,
                    Gender = "Мужской",
                    RegistrationDate = DateTime.Now,
                    SyncCoins = 100,
                    StepsGoal = 10000,
                    WaterGoal = 2.5,
                    SleepGoal = 8.0,
                    NotificationsEnabled = true,
                    Theme = "Light",
                    AutoSync = true,
                    UnitsSystem = "Metric",
                    DailyReminder = true,
                    ReminderTime = "20:00"
                };
                users.Add(defaultUser);
                SaveUsers();
            }
        }
    }
}