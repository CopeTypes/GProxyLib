
using GProxyLib.api.scraper;

namespace GProxyLib
{
    public class GProxyLib
    {

        public static GProxyScraper Scraper()
        {
            return GProxyScraper.Create();
        }
    }
}