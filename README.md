# GProxyLib
GProxyLib is a simple library for using proxies in .NET projects

# Features
- [GProxy](https://github.com/CopeTypes/GProxyLib/blob/main/api/GProxy.cs) - Simple and Universal class for storing Http & Socks4/5 proxies
- Built-in proxy scraper - [GProxyScraper](https://github.com/CopeTypes/GProxyLib/blob/main/api/scraper/GProxyScraper.cs)
- Built-in proxy checker - [GProxyChecker](https://github.com/CopeTypes/GProxyLib/blob/main/api/tester/GProxyTester.cs)


# Example Usage
            
            var scraper = GProxyLib.Scraper().From("custom_url", GProxyType.Http); - Creating a scraper
            var scraper = GProxyLib.Scraper().FromProxyScrape(GProxyType.Http, 5000, GProxyScraper.CountryCode.Any);
            var scraper = GProxyLib.Scraper().FromGeonode(GProxyType.Http, 5000, GProxyScraper.CountryCode.Any);

            (more scraper arguments detailed in documentation)

            var results = scraper.Scrape(); - Collect proxies from scraper

            var tester = GProxyLib.Tester(results, "http://google.com"); - Create a new tester
            var tester = GProxyLib.Tester(results, "http://google.com", 500, 10); - Create a new tester with specific max threads and timeout
            
            var working = await tester.Test(); - Collect working proxies from tester
