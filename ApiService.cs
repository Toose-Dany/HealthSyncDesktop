using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HealthSync
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "http://localhost:8000"; // URL твоего сервера

        public ApiService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        // Модели для API
        public class HealthMetric
        {
            public int heart_rate { get; set; }
            public int systolic { get; set; }
            public int diastolic { get; set; }
            public int steps { get; set; }
            public double water { get; set; }
            public double sleep { get; set; }
            public int calories { get; set; }
        }

        public class SyncCoinResponse
        {
            public int balance { get; set; }
        }

        public class StepsResponse
        {
            public int total_steps { get; set; }
            public int calories { get; set; }
            public int sync_coins_added { get; set; }
        }

        public class WaterResponse
        {
            public double total_water { get; set; }
            public int sync_coins_added { get; set; }
        }

        // Получить сегодняшние метрики
        public async Task<HealthMetric> GetTodayMetricsAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/users/{userId}/metrics/today");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<HealthMetric>(json);
                }
                return new HealthMetric(); // Пустые метрики
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка: {ex.Message}");
                return new HealthMetric();
            }
        }

        // Обновить все метрики - ЭТОТ МЕТОД НУЖЕН
        public async Task<bool> UpdateMetricsAsync(int userId, HealthMetric metrics)
        {
            try
            {
                var json = JsonConvert.SerializeObject(metrics);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"/users/{userId}/metrics/", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка: {ex.Message}");
                return false;
            }
        }

        // Добавить шаги
        public async Task<StepsResponse> AddStepsAsync(int userId, int stepsToAdd)
        {
            try
            {
                var content = new StringContent("", Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"/users/{userId}/steps/add?steps_to_add={stepsToAdd}", content);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<StepsResponse>(json);
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка: {ex.Message}");
                return null;
            }
        }

        // Добавить воду
        public async Task<WaterResponse> AddWaterAsync(int userId, double waterToAdd)
        {
            try
            {
                var content = new StringContent("", Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"/users/{userId}/water/add?water_to_add={waterToAdd.ToString(System.Globalization.CultureInfo.InvariantCulture)}", content);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<WaterResponse>(json);
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка: {ex.Message}");
                return null;
            }
        }

        // Получить баланс SyncCoin
        public async Task<int> GetSyncCoinBalanceAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/users/{userId}/sync-coins/balance");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<SyncCoinResponse>(json);
                    return result.balance;
                }
                return 150; // Значение по умолчанию
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка: {ex.Message}");
                return 150;
            }
        }
    }
}