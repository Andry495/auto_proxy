using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using ProxyCollector.Models;

namespace ProxyCollector.Services
{
    public class ProxyChecker
    {
        private readonly HttpClient _httpClient;
        private readonly SemaphoreSlim _semaphore;
        private readonly SettingsManager _settingsManager;
        private const int MaxConcurrentChecks = 10;

        public ProxyChecker(SettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(_settingsManager.GetIntSetting("TestTimeout", 10));
            _semaphore = new SemaphoreSlim(MaxConcurrentChecks, MaxConcurrentChecks);
        }

        public async Task<List<ProxyServer>> CheckProxiesAsync(List<ProxyServer> proxies, IProgress<ProxyCheckProgress>? progress = null)
        {
            var checkedProxies = new List<ProxyServer>();
            var totalCount = proxies.Count;
            var completedCount = 0;

            var tasks = new List<Task<ProxyServer>>();

            foreach (var proxy in proxies)
            {
                tasks.Add(CheckSingleProxyAsync(proxy, progress, totalCount, completedCount++));
            }

            var results = await Task.WhenAll(tasks);
            checkedProxies.AddRange(results);

            return checkedProxies;
        }

        private async Task<ProxyServer> CheckSingleProxyAsync(ProxyServer proxy, IProgress<ProxyCheckProgress>? progress, int totalCount, int completedCount)
        {
            await _semaphore.WaitAsync();
            
            try
            {
                // Отправляем информацию о начале проверки
                progress?.Report(new ProxyCheckProgress
                {
                    Completed = completedCount,
                    Total = totalCount,
                    CurrentProxy = proxy.FullAddress,
                    IsComplete = false
                });

                var result = await CheckProxyAsync(proxy);
                
                // Отправляем информацию о завершении проверки
                progress?.Report(new ProxyCheckProgress
                {
                    Completed = completedCount + 1,
                    Total = totalCount,
                    CurrentProxy = proxy.FullAddress,
                    IsComplete = completedCount + 1 == totalCount
                });

                return result;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task<ProxyServer> CheckProxyAsync(ProxyServer proxy)
        {
            var startTime = DateTime.Now;
            var testUrl = _settingsManager.GetStringSetting("TestUrl", "https://2ip.ru");
            var timeout = _settingsManager.GetIntSetting("TestTimeout", 10);
            
            try
            {
                var webProxy = CreateWebProxy(proxy);
                var handler = new HttpClientHandler()
                {
                    Proxy = webProxy,
                    UseProxy = true
                };

                using var client = new HttpClient(handler);
                client.Timeout = TimeSpan.FromSeconds(timeout);

                // Адаптируем URL для тестирования в зависимости от типа прокси
                var testUrlToUse = GetTestUrlForProxyType(proxy.Type, testUrl);
                
                // Тестируем подключение к настроенному URL
                var response = await client.GetAsync(testUrlToUse);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseTime = (int)(DateTime.Now - startTime).TotalMilliseconds;
                    proxy.IsAvailable = true;
                    proxy.ResponseTime = responseTime;
                    proxy.LastChecked = DateTime.Now;
                }
                else
                {
                    proxy.IsAvailable = false;
                    proxy.ResponseTime = -1;
                    proxy.LastChecked = DateTime.Now;
                }
            }
            catch (Exception)
            {
                proxy.IsAvailable = false;
                proxy.ResponseTime = -1;
                proxy.LastChecked = DateTime.Now;
            }

            return proxy;
        }

        private WebProxy CreateWebProxy(ProxyServer proxy)
        {
            var proxyUri = proxy.Type switch
            {
                ProxyType.HTTP => $"http://{proxy.IpAddress}:{proxy.Port}",
                ProxyType.HTTPS => $"https://{proxy.IpAddress}:{proxy.Port}",
                ProxyType.SOCKS4 => $"socks4://{proxy.IpAddress}:{proxy.Port}",
                ProxyType.SOCKS5 => $"socks5://{proxy.IpAddress}:{proxy.Port}",
                _ => $"http://{proxy.IpAddress}:{proxy.Port}"
            };

            return new WebProxy(proxyUri);
        }

        private string GetTestUrlForProxyType(ProxyType proxyType, string baseUrl)
        {
            return proxyType switch
            {
                ProxyType.HTTP => baseUrl.Replace("https://", "http://"),
                ProxyType.HTTPS => baseUrl.Replace("http://", "https://"),
                ProxyType.SOCKS4 => baseUrl, // SOCKS4/5 могут работать с любым протоколом
                ProxyType.SOCKS5 => baseUrl,
                _ => baseUrl
            };
        }

        public async Task<bool> TestProxyConnection(ProxyServer proxy)
        {
            try
            {
                var webProxy = CreateWebProxy(proxy);
                var handler = new HttpClientHandler()
                {
                    Proxy = webProxy,
                    UseProxy = true
                };

                using var client = new HttpClient(handler);
                client.Timeout = TimeSpan.FromSeconds(5);

                var testUrl = _settingsManager.GetStringSetting("TestUrl", "https://2ip.ru");
                var testUrlToUse = GetTestUrlForProxyType(proxy.Type, testUrl);
                
                var response = await client.GetAsync(testUrlToUse);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
            _semaphore?.Dispose();
        }
    }

    public class ProxyCheckProgress
    {
        public int Completed { get; set; }
        public int Total { get; set; }
        public string CurrentProxy { get; set; } = string.Empty;
        public bool IsComplete { get; set; }
        public double ProgressPercentage => Total > 0 ? (double)Completed / Total * 100 : 0;
    }
}
