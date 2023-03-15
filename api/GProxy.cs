using System;
using System.Net;
using System.Net.Http;

namespace GProxyLib.api
{
    public class GProxy
    { // Simple class for storing and using a proxy
        public string Ip { get; set; }
        public string Port { get; set; }
        public GProxyType Protocol { get; set; }
        
        
        
        
        public GProxy(string ip, string port, string type)
        {
            if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(port) || string.IsNullOrEmpty(type)) return;
            switch (type.ToLower())
            {
                case "http":
                    Protocol = GProxyType.Http;
                    break;
                case "socks4":
                    Protocol = GProxyType.Socks4;
                    break;
                case "socks5":
                    Protocol = GProxyType.Socks5;
                    break;
            }
            Ip = ip;
            Port = port;
        }
        
        // todo helper methods

        /// <summary>
        /// Returns this as an instance of WebProxy
        /// </summary>
        /// <returns>WebProxy</returns>
        /// <exception cref="ArgumentOutOfRangeException">Invalid proxy protocol</exception>
        public WebProxy AsWebProxy()
        { 
            string proxyStr = "";
            switch (Protocol)
            {
                case GProxyType.Http:
                    proxyStr += "http://";
                    break;
                case GProxyType.Socks4:
                    proxyStr += "socks4://";
                    break;
                case GProxyType.Socks5:
                    proxyStr += "socks5://";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            proxyStr += Ip;
            return new WebProxy(proxyStr, int.Parse(Port));
        }

        /// <summary>
        /// Creates a basic HttpClient that will use this proxy
        /// </summary>
        /// <returns>HttpClient</returns>
        public HttpClient MakeHttpClient()
        {
            var client = new HttpClient(new HttpClientHandler
            {
                Proxy = AsWebProxy(),
                UseProxy = true
            });
            client.Timeout = TimeSpan.FromSeconds(5);
            return client;
        }

        /// <summary>
        /// Creates a basic WebClient that will use this proxy
        /// </summary>
        /// <returns>WebClient</returns>
        public WebClient MakeWebClient()
        {
            var client = new WebClient();
            client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/111.0.0.0 Safari/537.36");
            client.Proxy = AsWebProxy();
            return client;
        }
    }
}