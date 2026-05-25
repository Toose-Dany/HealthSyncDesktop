using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HealthSync
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public ApiClient(string baseUrl)
        {
            _baseUrl = baseUrl;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        // ========== АУТЕНТИФИКАЦИЯ ==========

        public async Task<(bool success, UserApiModel user, string error)> Login(string username, string password)
        {
            try
            {
                var loginData = new { username, password };
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/login", loginData);

                if (response.IsSuccessStatusCode)
                {
                    var user = await response.Content.ReadFromJsonAsync<UserApiModel>();
                    return (true, user, null);
                }

                var error = await response.Content.ReadAsStringAsync();
                return (false, null, error);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }

        public async Task<(bool success, string error)> Register(string username, string email, string password,
            double height, double weight, int age, string gender)
        {
            try
            {
                var dateOfBirth = DateTime.Now.AddYears(-age).ToString("yyyy-MM-dd");

                var registerData = new
                {
                    email,
                    username,
                    password,
                    full_name = username,
                    date_of_birth = dateOfBirth,
                    gender = gender == "Мужской" ? "male" : "female",
                    height,
                    weight,
                    activity_level = "moderate"
                };

                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/users", registerData);

                if (response.IsSuccessStatusCode)
                    return (true, null);

                var error = await response.Content.ReadAsStringAsync();
                return (false, error);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        // ========== ОБНОВЛЕНИЕ ДАННЫХ ==========

        public async Task<bool> UpdateSteps(int userId, int steps)
        {
            try
            {
                var data = new { user_id = userId, steps };
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/steps", data);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public async Task<bool> UpdateWater(int userId, double waterLiters)
        {
            try
            {
                int waterMl = (int)(waterLiters * 1000);
                var data = new { user_id = userId, water_ml = waterMl };
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/water", data);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public async Task<bool> UpdateSleep(int userId, double sleepHours)
        {
            try
            {
                var data = new { user_id = userId, sleep_hours = sleepHours };
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/sleep", data);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public async Task<bool> UpdateVitals(int userId, int heartRate, int systolic, int diastolic)
        {
            try
            {
                var data = new { user_id = userId, heart_rate = heartRate, systolic, diastolic };
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/vitals", data);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public async Task<bool> UpdateWeight(int userId, double weight)
        {
            try
            {
                var data = new { user_id = userId, weight };
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/weight", data);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public async Task<bool> UpdateHeight(int userId, double height)
        {
            try
            {
                var data = new { user_id = userId, height };
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/height", data);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public async Task<bool> UpdateGoals(int userId, int? stepsGoal, double? waterGoal, double? sleepGoal, int? caloriesGoal)
        {
            try
            {
                var data = new
                {
                    user_id = userId,
                    steps_goal = stepsGoal,
                    water_goal_ml = waterGoal.HasValue ? (int?)(waterGoal.Value * 1000) : null,
                    sleep_goal = sleepGoal,
                    calories_goal = caloriesGoal
                };
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/goals", data);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public async Task<bool> UpdateSettings(int userId, string city, string theme, bool notifications, bool autoSync, bool dailyReminder, string reminderTime)
        {
            try
            {
                var data = new
                {
                    user_id = userId,
                    city,
                    theme,
                    notifications_enabled = notifications,
                    auto_sync = autoSync,
                    daily_reminder = dailyReminder,
                    reminder_time = reminderTime
                };
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/settings", data);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        // ========== ПОЛУЧЕНИЕ ДАННЫХ ==========

        public async Task<UserApiModel> GetUserData(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/users/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<UserApiModel>();
                }
                return null;
            }
            catch { return null; }
        }

        public async Task<WeatherResponse> GetWeather(string city)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/weather/{city}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<WeatherResponse>();
                }
                return null;
            }
            catch { return null; }
        }

        public async Task<HistoryResponse> GetHistory(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/history/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<HistoryResponse>();
                }
                return null;
            }
            catch { return null; }
        }

        public async Task<bool> HealthCheck()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/");
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }
    }

    // ========== МОДЕЛИ ДЛЯ ОТВЕТОВ ==========

    public class UserApiModel
    {
        public int id { get; set; }
        public string email { get; set; }
        public string username { get; set; }
        public double height { get; set; }
        public double weight { get; set; }
        public int age { get; set; }
        public int sync_coins { get; set; }
        public int steps { get; set; }
        public double water { get; set; }
        public double sleep { get; set; }
        public int heart_rate { get; set; }
        public int systolic { get; set; }
        public int diastolic { get; set; }
        public int steps_goal { get; set; }
        public double water_goal { get; set; }
        public double sleep_goal { get; set; }
        public int calories_goal { get; set; }
        public string city { get; set; }
        public string theme { get; set; }
        public string gender { get; set; }
        public bool notifications_enabled { get; set; }
        public bool auto_sync { get; set; }
        public bool daily_reminder { get; set; }
        public string reminder_time { get; set; }
    }

    public class WeatherResponse
    {
        public double temperature { get; set; }
        public string condition { get; set; }
        public string recommendation { get; set; }
    }

    public class HistoryResponse
    {
        public List<int> steps_history { get; set; }
        public List<double> water_history { get; set; }
        public List<double> sleep_history { get; set; }
        public List<int> calories_history { get; set; }
    }
}