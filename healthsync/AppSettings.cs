using System;
using System.IO;
using System.Text.Json;

namespace HealthSync
{
    public class AppSettings
    {
        private static readonly string SettingsFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "HealthSync", "settings.json");

        public bool IsDarkTheme { get; set; }
        public string Username { get; set; }
        public string Token { get; set; }
        public string ServerUrl { get; set; } = "http://localhost:8000";
        public bool RememberMe { get; set; }
        public DateTime LastSync { get; set; }

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsFile))
                {
                    string json = File.ReadAllText(SettingsFile);
                    return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch { }
            return new AppSettings();
        }

        public void Save()
        {
            try
            {
                string dir = Path.GetDirectoryName(SettingsFile);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                string json = JsonSerializer.Serialize(this);
                File.WriteAllText(SettingsFile, json);
            }
            catch { }
        }
    }
}