using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ProxyCollector.Services
{
    public class SettingsManager
    {
        private readonly string _settingsPath;
        private readonly Dictionary<string, string> _settings;

        public SettingsManager()
        {
            _settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.txt");
            _settings = new Dictionary<string, string>();
            LoadSettings();
        }

        public void SetSetting(string key, string value)
        {
            _settings[key] = value;
            SaveSettings();
        }

        public string GetSetting(string key, string defaultValue = "")
        {
            return _settings.ContainsKey(key) ? _settings[key] : defaultValue;
        }

        public bool GetBoolSetting(string key, bool defaultValue = false)
        {
            var value = GetSetting(key, defaultValue.ToString());
            return bool.TryParse(value, out bool result) ? result : defaultValue;
        }

        public int GetIntSetting(string key, int defaultValue = 0)
        {
            var value = GetSetting(key, defaultValue.ToString());
            return int.TryParse(value, out int result) ? result : defaultValue;
        }

        public void SetBoolSetting(string key, bool value)
        {
            SetSetting(key, value.ToString());
        }

        public void SetIntSetting(string key, int value)
        {
            SetSetting(key, value.ToString());
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var lines = File.ReadAllLines(_settingsPath);
                    foreach (var line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                            continue;

                        var parts = line.Split('=', 2);
                        if (parts.Length == 2)
                        {
                            _settings[parts[0].Trim()] = parts[1].Trim();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке настроек: {ex.Message}");
            }
        }

        private void SaveSettings()
        {
            try
            {
                var lines = new List<string>
                {
                    "# Настройки Proxy Collector",
                    "# Создано: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    ""
                };

                foreach (var setting in _settings.OrderBy(s => s.Key))
                {
                    lines.Add($"{setting.Key}={setting.Value}");
                }

                File.WriteAllLines(_settingsPath, lines);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении настроек: {ex.Message}");
            }
        }

        public void ResetToDefaults()
        {
            _settings.Clear();
            SetDefaultSettings();
        }

        private void SetDefaultSettings()
        {
            // Настройки автообновления
            SetBoolSetting("AutoRefresh", false);
            SetIntSetting("RefreshInterval", 30); // секунды
            
            // Настройки фонового режима
            SetBoolSetting("BackgroundMode", false);
            SetIntSetting("BackgroundInterval", 300); // секунды
            
            // Настройки парсинга
            SetIntSetting("MaxProxiesPerSource", 100);
            SetIntSetting("ConnectionTimeout", 10); // секунды
            SetIntSetting("CheckTimeout", 5); // секунды
            
            // Настройки интерфейса
            SetBoolSetting("MinimizeToTray", true);
            SetBoolSetting("ShowNotifications", true);
            SetIntSetting("LogEntriesLimit", 200);
            
            // Настройки экспорта
            SetStringSetting("DefaultExportFormat", "txt");
            SetStringSetting("DefaultExportPath", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "export"));
        }

        public void SetStringSetting(string key, string value)
        {
            SetSetting(key, value);
        }

        public string GetStringSetting(string key, string defaultValue = "")
        {
            return GetSetting(key, defaultValue);
        }
    }
}
