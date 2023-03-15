using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GProxyLib.api.scraper
{
    
    // scrape proxies from popular sites, and from custom urls
    public class GProxyScraper
    {

        private enum ScrapeType
        {
            User,
            ProxyScrape,
            Geonode
        }

        public enum HttpAnonType
        {
            Anonymous,
            Elite,
            None
        }
        
        public enum CountryCode { Any, AF, AX, AL, DZ, AS, AD, AO, AI, AQ, AG, AR, AM, AW, AU, AT, AZ, BS, BH, BD, BB, BY, BE, BZ, BJ, BM, BT, BO, BA, BW, BV, BR, IO, BN, BG, BF, BI, KH, CM, CA, CV, KY, CF, TD, CL, CN, CX, CC, CO, KM, CG, CD, CK, CR, CI, HR, CU, CY, CZ, DK, DJ, DM, DO, EC, EG, SV, GQ, ER, EE, ET, FK, FO, FJ, FI, FR, GF, PF, TF, GA, GM, GE, DE, GH, GI, GR, GL, GD, GP, GU, GT, GG, GN, GW, GY, HT, HM, VA, HN, HK, HU, IS, IN, ID, IR, IQ, IE, IM, IL, IT, JM, JP, JE, JO, KZ, KE, KI, KP, KR, KW, KG, LA, LV, LB, LS, LR, LY, LI, LT, LU, MO, MK, MG, MW, MY, MV, ML, MT, MH, MQ, MR, MU, YT, MX, FM, MD, MC, MN, ME, MS, MA, MZ, MM, NA, NR, NP, NL, AN, NC, NZ, NI, NE, NG, NU, NF, MP, NO, OM, PK, PW, PS, PA, PG, PY, PE, PH, PN, PL, PT, PR, QA, RE, RO, RU, RW, SH, KN, LC, PM, VC, WS, SM, ST, SA, SN, RS, SC, SL, SG, SK, SI, SB, SO, ZA, GS, ES, LK, SD, SR, SJ, SZ, SE, CH, SY, TW, TJ, TZ, TH, TL, TG, TK, TO, TT, TN, TR, TM, TC, TV, UG, UA, AE, GB, US, UM, UY, UZ, VU, VE, VN, VG, VI, WF, EH, YE, ZM, ZW }


        private string targetUrl;
        private GProxyType targetType;
        private ScrapeType scrapeType;
        private int maxProxyWait;
        

        /// <summary>
        /// Creates a new GProxyScraper without any options set.
        /// </summary>
        /// <returns></returns>
        public static GProxyScraper Create()
        {
            return new GProxyScraper();
        }

        /// <summary>
        /// Tell the scraper to get proxies from a custom url
        /// </summary>
        /// <param name="url">The custom url to get proxies from</param>
        /// <param name="proxyType">Which proxy type (GProxyType) the url will contain</param>
        /// <returns>GProxyScraper</returns>
        /// <exception cref="ArgumentException">The url provided was invalid</exception>
        public GProxyScraper From(string url, GProxyType proxyType)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentException("Invalid url provided.");
            targetUrl = url;
            targetType = proxyType;
            return this;
        }

        //https://proxyscrape.com/free-proxy-list
        /// <summary>
        /// Tell the scraper to get proxies from ProxyScrape
        /// </summary>
        /// <param name="proxyType">Which proxy type to get</param>
        /// <param name="timeout">The max time in milliseconds the proxy can take to reply</param>
        /// <param name="countryCode">Filter results by a specific country</param>
        /// <param name="ssl">Include HTTPS proxies</param>
        /// <param name="anonType">Set the anonymity level for HTTP proxies</param>
        /// <returns>GProxyScraper</returns>
        /// <exception cref="ArgumentException">Invalid timeout provided</exception>
        /// <exception cref="ArgumentOutOfRangeException">Invalid proxy type provided</exception>
        public GProxyScraper FromProxyScrape(GProxyType proxyType, int timeout, CountryCode countryCode, bool ssl = false, HttpAnonType anonType = HttpAnonType.None)
        {
            if (timeout == -1) throw new ArgumentException("Invalid timeout.");
            if (timeout < 1000) timeout *= 1000; // if seconds is passed by mistake
            maxProxyWait = timeout;
            var requestUrl = "https://api.proxyscrape.com/v2/?request=getproxies";
            switch (proxyType)
            {
                case GProxyType.Http:
                    requestUrl += "&protocol=http";
                    targetType = GProxyType.Http;
                    break;
                case GProxyType.Socks4:
                    requestUrl += "&protocol=socks4";
                    targetType = GProxyType.Socks4;
                    break;
                case GProxyType.Socks5:
                    requestUrl += "&protocol=socks5";
                    targetType = GProxyType.Socks5;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(proxyType), proxyType, "Invalid GProxyType");
            }

            requestUrl += $@"&timeout={timeout}";
            if (countryCode != CountryCode.Any) requestUrl += $@"&country={countryCode.ToString()}";
            
            
            if (proxyType.Equals(GProxyType.Http))
            {
                if (ssl) requestUrl += "&ssl=1";
                switch (anonType)
                {
                    case HttpAnonType.Elite:
                        requestUrl += "&anonymity=elite";
                        break;
                    case HttpAnonType.Anonymous:
                        requestUrl += "&anonymity=anonymous";
                        break;
                    case HttpAnonType.None:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(anonType), anonType, "Invalid anonymity type.");
                }
            }

            targetUrl = requestUrl;
            return this;
        }

        public enum GeonodeSpeed
        {
            Any,
            Fast,
            Medium,
            Slow
        }
        
        
        //https://geonode.com/free-proxy-list
        /// <summary>
        /// Tell the scraper to get proxies from Geonode
        /// </summary>
        /// <param name="proxyType">Which proxy type to get</param>
        /// <param name="timeout">The max time in milliseconds the proxy can take to reply</param>
        /// <param name="countryCode">Filter results by a specific country</param>
        /// <param name="limit">Limit how many results are returned by the api (default is 500)</param>
        /// <param name="page">Request a specific page from the api (default is 1)</param>
        /// <param name="speed">Filter results by a specific speed (default is any)</param>
        /// <param name="uptime">Filter results by how long they've been up (default is any, valid values from 10-100)</param>
        /// <param name="lastCheck">Filter results by how long they've been checked (default is any, valid values in minutes from 1-10, then 20, 30, 40 etc until 60)</param>
        /// <param name="anonType">Set the anonymity level for HTTP proxies</param>
        /// <param name="googlePassed">Ignore proxies flagged by google (default is false)</param>
        /// <returns>GProxyScraper</returns>
        /// <exception cref="ArgumentNullException">Invalid uptime or lastCheck</exception>
        /// <exception cref="ArgumentOutOfRangeException">Invalid page or timeout</exception>
        public GProxyScraper FromGeonode(GProxyType proxyType, int timeout, CountryCode countryCode, int limit = 500, int page = 1, GeonodeSpeed speed = GeonodeSpeed.Any, 
            string uptime = "any", string lastCheck = "any", HttpAnonType anonType = HttpAnonType.None, bool googlePassed = false)
        {
            if (timeout == -1) throw new ArgumentNullException(nameof(timeout));
            if (timeout < 1000) timeout *= 1000; // if seconds is passed by mistake
            maxProxyWait = timeout;
            if (limit == -1) throw new ArgumentNullException(nameof(limit));
            if (page == -1) throw new ArgumentNullException(nameof(page));
            if (string.IsNullOrEmpty(uptime)) throw new ArgumentNullException(nameof(uptime));
            if (string.IsNullOrEmpty(lastCheck)) throw new ArgumentNullException(nameof(lastCheck));

            var baseUrl = "https://proxylist.geonode.com/api/proxy-list?";
            baseUrl += $@"limit={limit}&page={page}&sort_by=lastChecked&sort_type=desc"; // sort_by and sort_type are hardcoded for the best lol

            if (lastCheck != "any") baseUrl += $@"&filterLastChecked={lastCheck};";
            if (uptime != "any") baseUrl += $@"&filterUpTime={uptime}";
            if (googlePassed) baseUrl += "&google=true";
            
            if (speed != GeonodeSpeed.Any) // handle speed
            {
                baseUrl += "&speed=";
                switch (speed)
                {
                    case GeonodeSpeed.Fast:
                        baseUrl += "fast";
                        break;
                    case GeonodeSpeed.Medium:
                        baseUrl += "medium";
                        break;
                    case GeonodeSpeed.Slow:
                        baseUrl += "slow";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(speed), speed, null);
                }
            }
            
            if (countryCode != CountryCode.Any) baseUrl += $@"&country={countryCode.ToString()}";
            
            baseUrl += "&protocols="; // handle proxy type
            switch (proxyType)
            {
                case GProxyType.Http:
                    baseUrl += "http";
                    break;
                case GProxyType.Socks4:
                    baseUrl += "socks4";
                    break;
                case GProxyType.Socks5:
                    baseUrl += "socks5";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(proxyType), proxyType, "GProxy type is invalid.");
            }

            if (anonType != HttpAnonType.None && proxyType.Equals(GProxyType.Http))
            {
                baseUrl += "&anonymityLevel=";
                switch (anonType)
                {
                    case HttpAnonType.Anonymous:
                        baseUrl += "anonymous";
                        break;
                    case HttpAnonType.Elite:
                        baseUrl += "elite";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(anonType), anonType, "Anonymous level type is invalid.");
                }
            }

            targetUrl = baseUrl;
            targetType = proxyType;
            scrapeType = ScrapeType.Geonode;
            return this;
        }
        
        private class GeonodeProxy
        {
            [JsonProperty("_id")] public string Id { get; set; }
            [JsonProperty("anonymityLevel")] public string AnonLevel { get; set; }
            [JsonProperty("asn")] public string Asn { get; set; }
            [JsonProperty("city")] public string City { get; set; }
            [JsonProperty("country")] public string Country { get; set; }
            [JsonProperty("created_at")] public string Creation { get; set; }
            [JsonProperty("google")] public string Google { get; set; }
            [JsonProperty("ip")] public string Ip { get; set; }
            [JsonProperty("isp")] public string Isp { get; set; }
            [JsonProperty("lastChecked")] public string LastCheck { get; set; }
            [JsonProperty("latency")] public double Latency { get; set; }
            [JsonProperty("org")] public string Org { get; set; }
            [JsonProperty("port")] public string Port { get; set; }
            [JsonProperty("protocols")] public List<string> Protocols { get; set; }
            [JsonProperty("region")] public string Region { get; set; }
            [JsonProperty("responseTime")] public int ResponseTime { get; set; }
            [JsonProperty("speed")] public int Speed { get; set; }
            [JsonProperty("updated_at")] public string LastUpdate { get; set; }
            [JsonProperty("upTime")] public float Uptime { get; set; }
            [JsonProperty("upTimeSuccessCount")] public int UptimeSuccess { get; set; }
            [JsonProperty("upTimeTryCount")] public int UptimeAttempts { get; set; }
            [JsonProperty("workingPercent")] public dynamic WorkingPercent { get; set; } // not sure what this is lol, it's always null
        }

        private class GeonodeResponse
        {
            [JsonProperty("data")] public List<GeonodeProxy> Proxies { get; set; }
            [JsonProperty("limit")] public int Limit { get; set; }
            [JsonProperty("page")] public int Page { get; set; }
            [JsonProperty("total")] public int Total { get; set; }
        }
        
        /// <summary>
        /// Scrapes proxies with the current configuration
        /// </summary>
        /// <returns>List(GProxy)</returns>
        /// <exception cref="ArgumentException">The source url isn't set</exception>
        /// <exception cref="Exception">The response from Geonode was empty</exception>
        /// <exception cref="ArgumentOutOfRangeException">The ScrapeType hasn't been configured.</exception>
        public List<GProxy> Scrape()
        {
            if (string.IsNullOrEmpty(targetUrl))
                throw new ArgumentException("Proxy source url is not specified, check your code.");

            var results = new List<GProxy>();
            switch (scrapeType)
            {
                case ScrapeType.User:
                case ScrapeType.ProxyScrape:
                {
                    using (var client = LegitClient())
                    {
                        var data = client.DownloadString(targetUrl);
                        var proxyList = data.Split('\n').ToList();
                        results.AddRange(from proxy in proxyList where !string.IsNullOrEmpty(proxy) && proxy.Contains(":") select proxy.Split(':') into d select new GProxy(d[0], d[1], targetType.ToString()));
                    }
                    
                    return results;
                }
                case ScrapeType.Geonode:
                    using (var client = LegitClient())
                    {
                        var data = client.DownloadString(targetUrl);
                        if (string.IsNullOrEmpty(data)) throw new Exception("Empty response from Geonode.");
                        var json = JsonConvert.DeserializeObject<GeonodeResponse>(data);
                        // filtering response time has to be done here since it's not provided in the "api"
                        results.AddRange(from gp in json.Proxies where gp.ResponseTime <= maxProxyWait select new GProxy(gp.Ip, gp.Port, targetType.ToString()));
                    }

                    return results;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Scrape a list of proxies from all built-in sources 
        /// </summary>
        /// <param name="proxyType">Which proxy type to get</param>
        /// <param name="timeout">The max time in milliseconds the proxy can take to reply</param>
        /// <param name="countryCode">Filter results by a specific country</param>
        /// <param name="ssl">Include HTTPS proxies</param>
        /// <param name="anonType">Set the anonymity level for HTTP proxies</param>
        /// <returns>Task(List(GProxy))</returns>
        public static async Task<List<GProxy>> ScrapeDefaults(GProxyType proxyType, int timeout, CountryCode countryCode,
            bool ssl = false, HttpAnonType anonType = HttpAnonType.None)
        {
            var tasks = new List<Task<List<GProxy>>>
            {
                Task.Run(() => Create().FromProxyScrape(proxyType, timeout, countryCode, ssl, anonType).Scrape()),
                Task.Run(() => Create().FromGeonode(proxyType, timeout, countryCode, 500, 1, 
                    GeonodeSpeed.Any, "any", "any", anonType).Scrape())
            };

            await Task.WhenAll(tasks);
            var results = new List<GProxy>();
            foreach (var t in tasks) results.AddRange(t.Result);
            return results;
        }
        
        private static WebClient LegitClient()
        {
            var client = new WebClient();
            client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/111.0.0.0 Safari/537.36");
            return client;
        }
    }
}