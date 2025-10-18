using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using HtmlAgilityPack;
using ProxyCollector.Models;

namespace ProxyCollector.Services
{
    public class ProxyParser
    {
        public event Action<string>? LogMessage;
        private readonly HttpClient _httpClient;
        private readonly List<string> _proxySources = new()
        {
            "https://hide-my-name.com/proxy-list/",
            "https://proxyscrape.com/free-proxy-list",
            "https://freeproxylist.ru/",
            "https://free-proxy-list.net/",
            "https://freeproxylist.ru/protocol/https",
            "https://freeproxylist.ru/protocol/socks",
            "https://proxyverity.com/free-proxy-list/",
            "https://ru.proxy-tools.com/proxy",
            "https://htmlweb.ru/analiz/proxy_list.php",
            "https://free.geonix.com/ru/"
        };

        public ProxyParser()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", 
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
        }

        public async Task<List<ProxyServer>> ParseAllSourcesAsync()
        {
            var allProxies = new List<ProxyServer>();
            var startTime = DateTime.Now;
            LogMessage?.Invoke($"Начало парсинга с {_proxySources.Count} источников");
            
            for (int i = 0; i < _proxySources.Count; i++)
            {
                var source = _proxySources[i];
                var sourceStartTime = DateTime.Now;
                
                try
                {
                    LogMessage?.Invoke($"Источник {i + 1}/{_proxySources.Count}: {source}");
                    LogMessage?.Invoke($"Подключение к {source}...");
                    
                    var proxies = await ParseSourceAsync(source);
                    var sourceDuration = (int)(DateTime.Now - sourceStartTime).TotalMilliseconds;
                    
                    LogMessage?.Invoke($"Получено {proxies.Count} прокси с {source} за {sourceDuration}мс");
                    
                    if (proxies.Count > 0)
                    {
                        var proxyTypes = proxies.GroupBy(p => p.Type).ToDictionary(g => g.Key, g => g.Count());
                        var typeInfo = string.Join(", ", proxyTypes.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
                        LogMessage?.Invoke($"Типы прокси: {typeInfo}");
                    }
                    
                    allProxies.AddRange(proxies);
                }
                catch (Exception ex)
                {
                    var sourceDuration = (int)(DateTime.Now - sourceStartTime).TotalMilliseconds;
                    LogMessage?.Invoke($"Ошибка при парсинге {source} за {sourceDuration}мс: {ex.Message}");
                    Console.WriteLine($"Ошибка при парсинге {source}: {ex.Message}");
                }
            }
            
            var totalDuration = (int)(DateTime.Now - startTime).TotalMilliseconds;
            LogMessage?.Invoke($"Парсинг завершен за {totalDuration}мс. Всего получено: {allProxies.Count} прокси");
            
            if (allProxies.Count > 0)
            {
                var totalTypes = allProxies.GroupBy(p => p.Type).ToDictionary(g => g.Key, g => g.Count());
                var totalTypeInfo = string.Join(", ", totalTypes.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
                LogMessage?.Invoke($"Общая статистика: {totalTypeInfo}");
            }
            
            return allProxies;
        }

        private async Task<List<ProxyServer>> ParseSourceAsync(string url)
        {
            var proxies = new List<ProxyServer>();
            
            try
            {
                // Проверяем, нужна ли пагинация для этого источника
                if (NeedsPagination(url))
                {
                    return await ParseSourceWithPaginationAsync(url);
                }

                LogMessage?.Invoke($"Загрузка HTML с {url}...");
                var html = await _httpClient.GetStringAsync(url);
                LogMessage?.Invoke($"HTML загружен, размер: {html.Length} символов");
                
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);

                if (url.Contains("hide-my-name.com"))
                {
                    LogMessage?.Invoke("Парсинг hide-my-name.com...");
                    var parsedProxies = ParseHideMyName(doc, url);
                    LogMessage?.Invoke($"Найдено {parsedProxies.Count} прокси на hide-my-name.com");
                    proxies.AddRange(parsedProxies);
                }
                else if (url.Contains("proxyscrape.com"))
                {
                    LogMessage?.Invoke("Парсинг proxyscrape.com...");
                    var parsedProxies = ParseProxyScrape(doc, url);
                    LogMessage?.Invoke($"Найдено {parsedProxies.Count} прокси на proxyscrape.com");
                    proxies.AddRange(parsedProxies);
                }
                else if (url.Contains("freeproxylist.ru"))
                {
                    LogMessage?.Invoke("Парсинг freeproxylist.ru...");
                    var parsedProxies = ParseFreeProxyListRu(doc, url);
                    LogMessage?.Invoke($"Найдено {parsedProxies.Count} прокси на freeproxylist.ru");
                    proxies.AddRange(parsedProxies);
                }
                else if (url.Contains("free-proxy-list.net"))
                {
                    LogMessage?.Invoke("Парсинг free-proxy-list.net...");
                    var parsedProxies = ParseFreeProxyListNet(doc, url);
                    LogMessage?.Invoke($"Найдено {parsedProxies.Count} прокси на free-proxy-list.net");
                    proxies.AddRange(parsedProxies);
                }
                else if (url.Contains("proxyverity.com"))
                {
                    LogMessage?.Invoke("Парсинг proxyverity.com...");
                    var parsedProxies = ParseProxyVerity(doc, url);
                    LogMessage?.Invoke($"Найдено {parsedProxies.Count} прокси на proxyverity.com");
                    proxies.AddRange(parsedProxies);
                }
                else if (url.Contains("proxy-tools.com"))
                {
                    LogMessage?.Invoke("Парсинг proxy-tools.com...");
                    var parsedProxies = ParseProxyTools(doc, url);
                    LogMessage?.Invoke($"Найдено {parsedProxies.Count} прокси на proxy-tools.com");
                    proxies.AddRange(parsedProxies);
                }
                else if (url.Contains("htmlweb.ru"))
                {
                    LogMessage?.Invoke("Парсинг htmlweb.ru...");
                    var parsedProxies = ParseHtmlWeb(doc, url);
                    LogMessage?.Invoke($"Найдено {parsedProxies.Count} прокси на htmlweb.ru");
                    proxies.AddRange(parsedProxies);
                }
                else if (url.Contains("geonix.com"))
                {
                    LogMessage?.Invoke("Парсинг geonix.com...");
                    var parsedProxies = ParseGeonix(doc, url);
                    LogMessage?.Invoke($"Найдено {parsedProxies.Count} прокси на geonix.com");
                    proxies.AddRange(parsedProxies);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при парсинге {url}: {ex.Message}");
            }

            return proxies;
        }

        private List<ProxyServer> ParseHideMyName(HtmlAgilityPack.HtmlDocument doc, string source)
        {
            var proxies = new List<ProxyServer>();
            
            try
            {
                var rows = doc.DocumentNode.SelectNodes("//table//tr");
                if (rows != null)
                {
                    foreach (var row in rows.Skip(1)) // пропускаем заголовок
                    {
                        var cells = row.SelectNodes("td");
                        if (cells != null && cells.Count >= 6)
                        {
                            var ip = cells[0].InnerText.Trim();
                            var portText = cells[1].InnerText.Trim();
                            var country = cells[2].InnerText.Trim();
                            var speedText = cells[3].InnerText.Trim();
                            var typeText = cells[4].InnerText.Trim();
                            var anonymity = cells[5].InnerText.Trim();

                            if (int.TryParse(portText, out int port) && 
                                int.TryParse(Regex.Replace(speedText, @"[^\d]", ""), out int speed))
                            {
                                var proxy = new ProxyServer
                                {
                                    IpAddress = ip,
                                    Port = port,
                                    Country = country,
                                    Speed = speed,
                                    Type = ParseProxyType(typeText),
                                    Source = source,
                                    LastChecked = DateTime.Now
                                };
                                proxies.Add(proxy);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при парсинге HideMyName: {ex.Message}");
            }

            return proxies;
        }

        private List<ProxyServer> ParseProxyScrape(HtmlAgilityPack.HtmlDocument doc, string source)
        {
            var proxies = new List<ProxyServer>();
            
            try
            {
                var textContent = doc.DocumentNode.InnerText;
                var lines = textContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var line in lines)
                {
                    if (line.Contains(":") && !line.Contains("http"))
                    {
                        var parts = line.Split(':');
                        if (parts.Length >= 2 && 
                            int.TryParse(parts[1].Trim(), out int port))
                        {
                            var proxy = new ProxyServer
                            {
                                IpAddress = parts[0].Trim(),
                                Port = port,
                                Type = ProxyType.HTTP,
                                Source = source,
                                LastChecked = DateTime.Now
                            };
                            proxies.Add(proxy);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при парсинге ProxyScrape: {ex.Message}");
            }

            return proxies;
        }

        private List<ProxyServer> ParseFreeProxyListRu(HtmlAgilityPack.HtmlDocument doc, string source)
        {
            var proxies = new List<ProxyServer>();
            
            try
            {
                var rows = doc.DocumentNode.SelectNodes("//table//tr");
                if (rows != null)
                {
                    foreach (var row in rows.Skip(1))
                    {
                        var cells = row.SelectNodes("td");
                        if (cells != null && cells.Count >= 3)
                        {
                            var ipPort = cells[0].InnerText.Trim();
                            var typeText = cells[1].InnerText.Trim();
                            var country = cells[2].InnerText.Trim();

                            if (ipPort.Contains(":"))
                            {
                                var parts = ipPort.Split(':');
                                if (parts.Length >= 2 && 
                                    int.TryParse(parts[1].Trim(), out int port))
                                {
                                    var proxy = new ProxyServer
                                    {
                                        IpAddress = parts[0].Trim(),
                                        Port = port,
                                        Country = country,
                                        Type = ParseProxyType(typeText),
                                        Source = source,
                                        LastChecked = DateTime.Now
                                    };
                                    proxies.Add(proxy);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при парсинге FreeProxyListRu: {ex.Message}");
            }

            return proxies;
        }

        private List<ProxyServer> ParseFreeProxyListNet(HtmlAgilityPack.HtmlDocument doc, string source)
        {
            var proxies = new List<ProxyServer>();
            
            try
            {
                var rows = doc.DocumentNode.SelectNodes("//table[@id='proxylisttable']//tr");
                if (rows != null)
                {
                    foreach (var row in rows.Skip(1))
                    {
                        var cells = row.SelectNodes("td");
                        if (cells != null && cells.Count >= 8)
                        {
                            var ip = cells[0].InnerText.Trim();
                            var portText = cells[1].InnerText.Trim();
                            var country = cells[3].InnerText.Trim();
                            var anonymity = cells[4].InnerText.Trim();
                            var isHttps = cells[6].InnerText.Trim().ToLower() == "yes";

                            if (int.TryParse(portText, out int port))
                            {
                                var proxy = new ProxyServer
                                {
                                    IpAddress = ip,
                                    Port = port,
                                    Country = country,
                                    Type = isHttps ? ProxyType.HTTPS : ProxyType.HTTP,
                                    Source = source,
                                    LastChecked = DateTime.Now
                                };
                                proxies.Add(proxy);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при парсинге FreeProxyListNet: {ex.Message}");
            }

            return proxies;
        }

        private ProxyType ParseProxyType(string typeText)
        {
            var type = typeText.ToLower();
            if (type.Contains("socks5"))
                return ProxyType.SOCKS5;
            else if (type.Contains("socks4") || type.Contains("socks4"))
                return ProxyType.SOCKS4;
            else if (type.Contains("https"))
                return ProxyType.HTTPS;
            else
                return ProxyType.HTTP;
        }

        // Методы для поддержки пагинации
        private bool NeedsPagination(string url)
        {
            return url.Contains("freeproxylist.ru") || 
                   url.Contains("proxyverity.com") || 
                   url.Contains("proxy-tools.com") ||
                   url.Contains("htmlweb.ru") ||
                   url.Contains("geonix.com");
        }

        private async Task<List<ProxyServer>> ParseSourceWithPaginationAsync(string url)
        {
            var allProxies = new List<ProxyServer>();
            var maxPages = 5; // Ограничиваем количество страниц для избежания долгого парсинга
            
            try
            {
                LogMessage?.Invoke($"Парсинг с пагинацией: {url}");
                
                for (int page = 1; page <= maxPages; page++)
                {
                    try
                    {
                        var pageUrl = GetPageUrl(url, page);
                        LogMessage?.Invoke($"Страница {page}: {pageUrl}");
                        
                        var html = await _httpClient.GetStringAsync(pageUrl);
                        var doc = new HtmlAgilityPack.HtmlDocument();
                        doc.LoadHtml(html);
                        
                        var pageProxies = new List<ProxyServer>();
                        
                        if (url.Contains("freeproxylist.ru"))
                        {
                            pageProxies = ParseFreeProxyListRu(doc, pageUrl);
                        }
                        else if (url.Contains("proxyverity.com"))
                        {
                            pageProxies = ParseProxyVerity(doc, pageUrl);
                        }
                        else if (url.Contains("proxy-tools.com"))
                        {
                            pageProxies = ParseProxyTools(doc, pageUrl);
                        }
                        else if (url.Contains("htmlweb.ru"))
                        {
                            pageProxies = ParseHtmlWeb(doc, pageUrl);
                        }
                        else if (url.Contains("geonix.com"))
                        {
                            pageProxies = ParseGeonix(doc, pageUrl);
                        }
                        
                        if (pageProxies.Count == 0)
                        {
                            LogMessage?.Invoke($"Страница {page}: прокси не найдены, завершение парсинга");
                            break;
                        }
                        
                        allProxies.AddRange(pageProxies);
                        LogMessage?.Invoke($"Страница {page}: найдено {pageProxies.Count} прокси");
                        
                        // Небольшая задержка между запросами
                        await Task.Delay(1000);
                    }
                    catch (Exception ex)
                    {
                        LogMessage?.Invoke($"Ошибка на странице {page}: {ex.Message}");
                        break;
                    }
                }
                
                LogMessage?.Invoke($"Парсинг с пагинацией завершен. Всего найдено: {allProxies.Count} прокси");
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke($"Ошибка при парсинге с пагинацией {url}: {ex.Message}");
            }
            
            return allProxies;
        }

        private string GetPageUrl(string baseUrl, int page)
        {
            if (baseUrl.Contains("freeproxylist.ru"))
            {
                return $"{baseUrl}?page={page}";
            }
            else if (baseUrl.Contains("proxyverity.com"))
            {
                return $"{baseUrl}?page={page}";
            }
            else if (baseUrl.Contains("proxy-tools.com"))
            {
                return $"{baseUrl}?page={page}";
            }
            else if (baseUrl.Contains("htmlweb.ru"))
            {
                return $"{baseUrl}?page={page}";
            }
            else if (baseUrl.Contains("geonix.com"))
            {
                return $"{baseUrl}?page={page}";
            }
            
            return baseUrl;
        }

        // Новые методы парсинга для дополнительных источников
        private List<ProxyServer> ParseProxyVerity(HtmlAgilityPack.HtmlDocument doc, string source)
        {
            var proxies = new List<ProxyServer>();
            
            try
            {
                var rows = doc.DocumentNode.SelectNodes("//table//tr");
                if (rows != null)
                {
                    foreach (var row in rows.Skip(1)) // Пропускаем заголовок
                    {
                        var cells = row.SelectNodes(".//td");
                        if (cells != null && cells.Count >= 4)
                        {
                            var ip = cells[0].InnerText.Trim();
                            var portText = cells[1].InnerText.Trim();
                            var country = cells[2].InnerText.Trim();
                            var protocol = cells[3].InnerText.Trim().ToUpper();

                            if (int.TryParse(portText, out int port))
                            {
                                var proxy = new ProxyServer
                                {
                                    IpAddress = ip,
                                    Port = port,
                                    Country = country,
                                    Type = ParseProxyType(protocol),
                                    Source = source,
                                    LastChecked = DateTime.Now
                                };
                                proxies.Add(proxy);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke($"Ошибка при парсинге ProxyVerity: {ex.Message}");
            }

            return proxies;
        }

        private List<ProxyServer> ParseProxyTools(HtmlAgilityPack.HtmlDocument doc, string source)
        {
            var proxies = new List<ProxyServer>();
            
            try
            {
                var rows = doc.DocumentNode.SelectNodes("//table//tr");
                if (rows != null)
                {
                    foreach (var row in rows.Skip(1)) // Пропускаем заголовок
                    {
                        var cells = row.SelectNodes(".//td");
                        if (cells != null && cells.Count >= 3)
                        {
                            var ip = cells[0].InnerText.Trim();
                            var portText = cells[1].InnerText.Trim();
                            var protocol = cells[2].InnerText.Trim().ToUpper();

                            if (int.TryParse(portText, out int port))
                            {
                                var proxy = new ProxyServer
                                {
                                    IpAddress = ip,
                                    Port = port,
                                    Type = ParseProxyType(protocol),
                                    Source = source,
                                    LastChecked = DateTime.Now
                                };
                                proxies.Add(proxy);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke($"Ошибка при парсинге ProxyTools: {ex.Message}");
            }

            return proxies;
        }

        private List<ProxyServer> ParseHtmlWeb(HtmlAgilityPack.HtmlDocument doc, string source)
        {
            var proxies = new List<ProxyServer>();
            
            try
            {
                var rows = doc.DocumentNode.SelectNodes("//table//tr");
                if (rows != null)
                {
                    foreach (var row in rows.Skip(1)) // Пропускаем заголовок
                    {
                        var cells = row.SelectNodes(".//td");
                        if (cells != null && cells.Count >= 2)
                        {
                            var ipPort = cells[0].InnerText.Trim();
                            var protocol = cells[1].InnerText.Trim().ToUpper();

                            var parts = ipPort.Split(':');
                            if (parts.Length == 2 && int.TryParse(parts[1], out int port))
                            {
                                var proxy = new ProxyServer
                                {
                                    IpAddress = parts[0],
                                    Port = port,
                                    Type = ParseProxyType(protocol),
                                    Source = source,
                                    LastChecked = DateTime.Now
                                };
                                proxies.Add(proxy);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke($"Ошибка при парсинге HtmlWeb: {ex.Message}");
            }

            return proxies;
        }

        private List<ProxyServer> ParseGeonix(HtmlAgilityPack.HtmlDocument doc, string source)
        {
            var proxies = new List<ProxyServer>();
            
            try
            {
                var rows = doc.DocumentNode.SelectNodes("//table//tr");
                if (rows != null)
                {
                    foreach (var row in rows.Skip(1)) // Пропускаем заголовок
                    {
                        var cells = row.SelectNodes(".//td");
                        if (cells != null && cells.Count >= 3)
                        {
                            var ip = cells[0].InnerText.Trim();
                            var portText = cells[1].InnerText.Trim();
                            var protocol = cells[2].InnerText.Trim().ToUpper();

                            if (int.TryParse(portText, out int port))
                            {
                                var proxy = new ProxyServer
                                {
                                    IpAddress = ip,
                                    Port = port,
                                    Type = ParseProxyType(protocol),
                                    Source = source,
                                    LastChecked = DateTime.Now
                                };
                                proxies.Add(proxy);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke($"Ошибка при парсинге Geonix: {ex.Message}");
            }

            return proxies;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
