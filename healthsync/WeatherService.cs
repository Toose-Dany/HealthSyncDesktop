using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace HealthSync
{
    public class WeatherService
    {
        private static readonly HttpClient client = new HttpClient();

        public class WeatherData
        {
            public string Condition { get; set; }
            public double Temperature { get; set; }
            public string Recommendation { get; set; }
        }

        public async Task<WeatherData> GetWeatherAsync()
        {
            // Open-Meteo API для Москвы (координаты 55.75, 37.62)
            string url = "https://api.open-meteo.com/v1/forecast?latitude=55.75&longitude=37.62&current_weather=true";

            var response = await client.GetStringAsync(url);
            var json = JObject.Parse(response);

            var current = json["current_weather"];

            double temperature = double.Parse(current["temperature"].ToString());

            // Определяем погоду по коду
            int weatherCode = int.Parse(current["weathercode"].ToString());
            string condition = weatherCode switch
            {
                0 => "Ясно",
                1 => "В основном ясно",
                2 => "Переменная облачность",
                3 => "Пасмурно",
                45 => "Туман",
                51 => "Легкая морось",
                61 => "Дождь",
                71 => "Снег",
                _ => "Облачно"
            };

            string recommendation = "";
            if (temperature < 10)
                recommendation = "🌬️ Прохладно, одевайтесь теплее";
            else if (temperature < 20)
                recommendation = "🌤️ Отличная погода для прогулки!";
            else if (temperature < 30)
                recommendation = "☀️ Тепло, не забывайте пить воду";
            else
                recommendation = "🔥 Жарко, пейте больше воды";

            return new WeatherData
            {
                Temperature = temperature,
                Condition = condition,
                Recommendation = recommendation
            };
        }
    }
}