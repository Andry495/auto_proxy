using System;

namespace ProxyCollector.Models
{
    public class ProxyServer
    {
        public string IpAddress { get; set; } = string.Empty;
        public int Port { get; set; }
        public ProxyType Type { get; set; }
        public string Country { get; set; } = string.Empty;
        public int Speed { get; set; } // в миллисекундах
        public DateTime LastChecked { get; set; }
        public bool IsAvailable { get; set; }
        public int ResponseTime { get; set; } // время отклика в мс
        public string Source { get; set; } = string.Empty; // источник получения

        public string FullAddress => $"{IpAddress}:{Port}";
        
        public string DisplayInfo => $"{FullAddress} ({Type}) - {Country} - {Speed}ms - {(IsAvailable ? "Online" : "Offline")}";
    }

    public enum ProxyType
    {
        HTTP,
        HTTPS,
        SOCKS4,
        SOCKS5
    }
}
