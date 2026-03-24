using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace YourWpfApp
{
    public class HealthSyncApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "http://localhost:8080"; // Ваш сервер

        public HealthSyncApiClient()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        // Модели данных (копируем из вашего сервера)
        public class UserCreate
        {
            public string email { get; set; }
            public string username { get; set; }
            public string password { get; set; }
            public string full_name { get; set; }
            public string date_of_birth { get; set; }
            public string gender { get; set; }
            public double? height { get; set; }
            public double? weight { get; set; }
            public string activity_level { get; set; } = "moderate";
        }

        public class DailyDataCreate
        {
            public int user_id { get; set; }
            public string date { get; set; }
            public int steps { get; set; } = 0;
            public int water_ml { get; set; } = 0;
            public string mood { get; set; }
            public double? sleep_hours { get; set; }
            public bool breakfast { get; set; } = false;
            public bool lunch { get; set; } = false;
            public bool dinner { get; set; } = false;
            public string notes { get; set; }
        }

        public class HabitCreate
        {
            public int user_id { get; set; }
            public int? template_id { get; set; }
            public string custom_name { get; set; }
            public int? target_value { get; set; }
            public string reminder_time { get; set; }
        }

        public class HabitCompletion
        {
            public int user_id { get; set; }
            public int habit_id { get; set; }
            public int? completed_value { get; set; }
        }

        // ============ МЕТОДЫ ДЛЯ РАБОТЫ С API ============

        // 1. Регистрация пользователя
        public async Task<(bool success, string message, int userId)> RegisterUser(UserCreate user)
        {
            try
            {
                var json = JsonSerializer.Serialize(user);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/users", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<JsonElement>(responseBody);
                    if (result.TryGetProperty("user", out var userData) &&
                        userData.TryGetProperty("id", out var id))
                    {
                        return (true, "Регистрация успешна!", id.GetInt32());
                    }
                    return (true, "Регистрация успешна!", 0);
                }
                else
                {
                    return (false, $"Ошибка: {response.StatusCode} - {responseBody}", 0);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Ошибка подключения: {ex.Message}", 0);
            }
        }

        // 2. Получение данных пользователя
        public async Task<string> GetUser(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/users/{userId}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        // 3. Отправка ежедневных данных
        public async Task<(bool success, string message)> UpdateDailyData(DailyDataCreate data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/daily", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Данные успешно отправлены!");
                }
                else
                {
                    return (false, $"Ошибка: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Ошибка подключения: {ex.Message}");
            }
        }

        // 4. Получение данных за день
        public async Task<string> GetDailyData(int userId, string date = null)
        {
            try
            {
                var url = $"/daily/{userId}";
                if (!string.IsNullOrEmpty(date))
                    url += $"?date_str={date}";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        // 5. Получение дашборда (главный экран)
        public async Task<string> GetDashboard(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/dashboard/{userId}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        // 6. Получение привычек пользователя
        public async Task<string> GetUserHabits(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/habits/{userId}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        // 7. Создание привычки
        public async Task<(bool success, string message)> CreateHabit(HabitCreate habit)
        {
            try
            {
                var json = JsonSerializer.Serialize(habit);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/habits", content);

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Привычка создана!");
                }
                else
                {
                    return (false, $"Ошибка: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Ошибка подключения: {ex.Message}");
            }
        }

        // 8. Отметить выполнение привычки
        public async Task<(bool success, string message)> CompleteHabit(HabitCompletion completion)
        {
            try
            {
                var json = JsonSerializer.Serialize(completion);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/habits/complete", content);

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Привычка выполнена!");
                }
                else
                {
                    return (false, $"Ошибка: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Ошибка подключения: {ex.Message}");
            }
        }

        // 9. Получение шаблонов привычек
        public async Task<string> GetHabitTemplates(string category = null)
        {
            try
            {
                var url = "/templates";
                if (!string.IsNullOrEmpty(category))
                    url += $"?category={category}";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        // 10. Получение аналитики
        public async Task<string> GetAnalytics(int userId, string startDate, string endDate)
        {
            try
            {
                var url = $"/analytics/{userId}?start_date={startDate}&end_date={endDate}";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}