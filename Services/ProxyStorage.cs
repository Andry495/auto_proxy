using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ProxyCollector.Models;

namespace ProxyCollector.Services
{
    public class ProxyStorage
    {
        private readonly string _storagePath;
        private readonly string _backupPath;

        public ProxyStorage(string storagePath = "proxies.txt")
        {
            _storagePath = storagePath;
            _backupPath = Path.ChangeExtension(storagePath, ".backup");
        }

        public async Task SaveProxiesAsync(List<ProxyServer> proxies)
        {
            try
            {
                // Создаем резервную копию
                if (File.Exists(_storagePath))
                {
                    File.Copy(_storagePath, _backupPath, true);
                }

                // Сохраняем в текстовом формате
                await SaveAsTextAsync(proxies);
                
                // Также сохраняем в JSON для более детальной информации
                await SaveAsJsonAsync(proxies);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при сохранении прокси: {ex.Message}", ex);
            }
        }

        private async Task SaveAsTextAsync(List<ProxyServer> proxies)
        {
            var lines = new List<string>
            {
                "# Proxy List - Generated: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                "# Format: IP:Port|Type|Country|Speed|Available|LastChecked|Source",
                ""
            };

            foreach (var proxy in proxies.OrderByDescending(p => p.IsAvailable).ThenBy(p => p.ResponseTime))
            {
                var line = $"{proxy.IpAddress}:{proxy.Port}|{proxy.Type}|{proxy.Country}|{proxy.Speed}|{proxy.IsAvailable}|{proxy.LastChecked:yyyy-MM-dd HH:mm:ss}|{proxy.Source}";
                lines.Add(line);
            }

            await File.WriteAllLinesAsync(_storagePath, lines);
        }

        private async Task SaveAsJsonAsync(List<ProxyServer> proxies)
        {
            var jsonPath = Path.ChangeExtension(_storagePath, ".json");
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var json = JsonSerializer.Serialize(proxies, jsonOptions);
            await File.WriteAllTextAsync(jsonPath, json);
        }

        public async Task<List<ProxyServer>> LoadProxiesAsync()
        {
            var proxies = new List<ProxyServer>();

            try
            {
                // Сначала пытаемся загрузить из JSON
                var jsonPath = Path.ChangeExtension(_storagePath, ".json");
                if (File.Exists(jsonPath))
                {
                    var json = await File.ReadAllTextAsync(jsonPath);
                    proxies = JsonSerializer.Deserialize<List<ProxyServer>>(json) ?? new List<ProxyServer>();
                }
                else if (File.Exists(_storagePath))
                {
                    // Если JSON нет, загружаем из текстового файла
                    proxies = await LoadFromTextAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке прокси: {ex.Message}");
                // Пытаемся загрузить из резервной копии
                if (File.Exists(_backupPath))
                {
                    try
                    {
                        proxies = await LoadFromTextAsync(_backupPath);
                    }
                    catch
                    {
                        // Если и резервная копия не работает, возвращаем пустой список
                        proxies = new List<ProxyServer>();
                    }
                }
            }

            return proxies;
        }

        private async Task<List<ProxyServer>> LoadFromTextAsync(string? filePath = null)
        {
            var path = filePath ?? _storagePath;
            var proxies = new List<ProxyServer>();
            var lines = await File.ReadAllLinesAsync(path);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                try
                {
                    var parts = line.Split('|');
                    if (parts.Length >= 7)
                    {
                        var ipPort = parts[0].Split(':');
                        if (ipPort.Length == 2 && int.TryParse(ipPort[1], out int port))
                        {
                            var proxy = new ProxyServer
                            {
                                IpAddress = ipPort[0],
                                Port = port,
                                Type = Enum.Parse<ProxyType>(parts[1]),
                                Country = parts[2],
                                Speed = int.TryParse(parts[3], out int speed) ? speed : 0,
                                IsAvailable = bool.TryParse(parts[4], out bool available) && available,
                                LastChecked = DateTime.TryParse(parts[5], out DateTime lastChecked) ? lastChecked : DateTime.MinValue,
                                Source = parts[6]
                            };
                            proxies.Add(proxy);
                        }
                    }
                }
                catch
                {
                    // Пропускаем некорректные строки
                    continue;
                }
            }

            return proxies;
        }

        public async Task ExportProxiesAsync(List<ProxyServer> proxies, string filePath, ExportFormat format)
        {
            switch (format)
            {
                case ExportFormat.Text:
                    await ExportAsTextAsync(proxies, filePath);
                    break;
                case ExportFormat.CSV:
                    await ExportAsCsvAsync(proxies, filePath);
                    break;
                case ExportFormat.JSON:
                    await ExportAsJsonAsync(proxies, filePath);
                    break;
            }
        }

        private async Task ExportAsTextAsync(List<ProxyServer> proxies, string filePath)
        {
            var lines = new List<string>();
            foreach (var proxy in proxies.Where(p => p.IsAvailable))
            {
                lines.Add($"{proxy.IpAddress}:{proxy.Port}");
            }
            await File.WriteAllLinesAsync(filePath, lines);
        }

        private async Task ExportAsCsvAsync(List<ProxyServer> proxies, string filePath)
        {
            var lines = new List<string>
            {
                "IP,Port,Type,Country,Speed,Available,LastChecked,Source"
            };

            foreach (var proxy in proxies)
            {
                var line = $"{proxy.IpAddress},{proxy.Port},{proxy.Type},{proxy.Country},{proxy.Speed},{proxy.IsAvailable},{proxy.LastChecked:yyyy-MM-dd HH:mm:ss},{proxy.Source}";
                lines.Add(line);
            }

            await File.WriteAllLinesAsync(filePath, lines);
        }

        private async Task ExportAsJsonAsync(List<ProxyServer> proxies, string filePath)
        {
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var json = JsonSerializer.Serialize(proxies, jsonOptions);
            await File.WriteAllTextAsync(filePath, json);
        }

        public void CleanupOldBackups(int maxBackups = 5)
        {
            try
            {
                var backupDir = Path.GetDirectoryName(_backupPath);
                if (string.IsNullOrEmpty(backupDir)) return;

                var backupFiles = Directory.GetFiles(backupDir, "*.backup")
                    .OrderByDescending(f => File.GetCreationTime(f))
                    .Skip(maxBackups);

                foreach (var file in backupFiles)
                {
                    File.Delete(file);
                }
            }
            catch
            {
                // Игнорируем ошибки очистки
            }
        }
    }

    public enum ExportFormat
    {
        Text,
        CSV,
        JSON
    }
}
