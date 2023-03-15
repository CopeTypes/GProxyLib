
using System.Collections.Generic;
using GProxyLib.api;
using GProxyLib.api.scraper;

namespace GProxyLib
{
    public class GProxyLib
    {

        public static GProxyScraper Scraper()
        {
            return GProxyScraper.Create();
        }

        public static GProxyTester Tester(IEnumerable<GProxy> proxies, string testUrl, int threads = 200, int timeout = 5)
        {
            return GProxyTester.Create(proxies, testUrl, threads, timeout);
        }
    }
}