using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace GProxyLib.api.scraper
{
    
    // scrape proxies from popular sites, and from custom urls
    public class GProxyScraper
    {

        private string targetUrl;
        private GProxyType targetType;

        public static GProxyScraper Create()
        {
            return new GProxyScraper();
        }

        public GProxyScraper From(string url, GProxyType proxyType)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentException("Invalid url provided.");
            targetType = proxyType;
            // todo impl
            

            return this;
        }

        public GProxyScraper FromProxyScrape(GProxyType proxyType, int timeout, string country, bool ssl = false, bool anon = false)
        {
            if (timeout == -1) throw new ArgumentException("Invalid timeout.");
            if (string.IsNullOrEmpty(country)) throw new ArgumentException("Invalid country.");
            var requestUrl = "https://api.proxyscrape.com/v2/?request=getproxies";
            switch (proxyType)
            {
                case GProxyType.Http:
                    requestUrl += "&protocol=http";
                    targetType = GProxyType.Http;
                    break;
                case GProxyType.Socks4:
                    requestUrl += "&procotol=socks4";
                    targetType = GProxyType.Socks4;
                    break;
                case GProxyType.Socks5:
                    requestUrl += "&protocol=socks5";
                    targetType = GProxyType.Socks5;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(proxyType), proxyType, "Invalid GProxyType");
            }

            requestUrl += $@"&timeout={timeout}&country={country}";
            
            if (proxyType.Equals(GProxyType.Http))
            {
                if (ssl) requestUrl += "&ssl=1";
                if (anon) requestUrl += "&anonymity=elite";
            }

            targetUrl = requestUrl;
            return this;
        }

        public List<GProxy> Scrape()
        {
            if (string.IsNullOrEmpty(targetUrl))
                throw new ArgumentException("Proxy source url is not specified, check your code.");

            var results = new List<GProxy>();
            using (var client = LegitClient())
            {
                var data = client.DownloadString(targetUrl);
                var proxyList = data.Split('\n').ToList();
                results.AddRange(from proxy in proxyList where !string.IsNullOrEmpty(proxy) && proxy.Contains(":") select proxy.Split(':') into d select new GProxy(d[0], d[1], targetType.ToString()));
            }

            return results;
        }



        private static WebClient LegitClient()
        {
            var client = new WebClient();
            client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/111.0.0.0 Safari/537.36");
            //todo other stuff?
            return client;
        }
    }
}