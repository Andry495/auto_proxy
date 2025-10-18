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
            "https://free-proxy-list.net/"
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
            LogMessage?.Invoke($"Начало парсинга с {_proxySources.Count} источников");
            
            foreach (var source in _proxySources)
            {
                try
                {
                    LogMessage?.Invoke($"Подключение к {source}...");
                    var proxies = await ParseSourceAsync(source);
                    LogMessage?.Invoke($"Получено {proxies.Count} прокси с {source}");
                    allProxies.AddRange(proxies);
                }
                catch (Exception ex)
                {
                    LogMessage?.Invoke($"Ошибка при парсинге {source}: {ex.Message}");
                    Console.WriteLine($"Ошибка при парсинге {source}: {ex.Message}");
                }
            }

            LogMessage?.Invoke($"Парсинг завершен. Всего получено {allProxies.Count} прокси");
            return allProxies;
        }

        private async Task<List<ProxyServer>> ParseSourceAsync(string url)
        {
            var proxies = new List<ProxyServer>();
            
            try
            {
                LogMessage?.Invoke($"Загрузка HTML с {url}...");
                var html = await _httpClient.GetStringAsync(url);
                LogMessage?.Invoke($"HTML загружен, размер: {html.Length} символов");
                
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);

                if (url.Contains("hide-my-name.com"))
                {
                    LogMessage?.Invoke("Парсинг hide-my-name.com...");
                    proxies.AddRange(ParseHideMyName(doc, url));
                }
                else if (url.Contains("proxyscrape.com"))
                {
                    LogMessage?.Invoke("Парсинг proxyscrape.com...");
                    proxies.AddRange(ParseProxyScrape(doc, url));
                }
                else if (url.Contains("freeproxylist.ru"))
                {
                    LogMessage?.Invoke("Парсинг freeproxylist.ru...");
                    proxies.AddRange(ParseFreeProxyListRu(doc, url));
                }
                else if (url.Contains("free-proxy-list.net"))
                {
                    LogMessage?.Invoke("Парсинг free-proxy-list.net...");
                    proxies.AddRange(ParseFreeProxyListNet(doc, url));
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

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
