using GProxyLib.api;
using GProxyLib.api.scraper;

namespace GProxyLib
{
    public class GProxyLib
    {


        public void Test()
        {
            var proxies = GProxyScraper
                .Create()
                .FromProxyScrape(GProxyType.Socks4, 5000, "US")
                //.From("url", GProxyType.Http)
                .Scrape();
        }
    }
}