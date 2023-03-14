using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        
        public enum CountryCode { Any, AF, AX, AL, DZ, AS, AD, AO, AI, AQ, AG, AR, AM, AW, AU, AT, AZ, BS, BH, BD, BB, BY, BE, BZ, BJ, BM, BT, BO, BA, BW, BV, BR, IO, BN, BG, BF, BI, KH, CM, CA, CV, KY, CF, TD, CL, CN, CX, CC, CO, KM, CG, CD, CK, CR, CI, HR, CU, CY, CZ, DK, DJ, DM, DO, EC, EG, SV, GQ, ER, EE, ET, FK, FO, FJ, FI, FR, GF, PF, TF, GA, GM, GE, DE, GH, GI, GR, GL, GD, GP, GU, GT, GG, GN, GW, GY, HT, HM, VA, HN, HK, HU, IS, IN, ID, IR, IQ, IE, IM, IL, IT, JM, JP, JE, JO, KZ, KE, KI, KP, KR, KW, KG, LA, LV, LB, LS, LR, LY, LI, LT, LU, MO, MK, MG, MW, MY, MV, ML, MT, MH, MQ, MR, MU, YT, MX, FM, MD, MC, MN, ME, MS, MA, MZ, MM, NA, NR, NP, NL, AN, NC, NZ, NI, NE, NG, NU, NF, MP, NO, OM, PK, PW, PS, PA, PG, PY, PE, PH, PN, PL, PT, PR, QA, RE, RO, RU, RW, SH, KN, LC, PM, VC, WS, SM, ST, SA, SN, RS, SC, SL, SG, SK, SI, SB, SO, ZA, GS, ES, LK, SD, SR, SJ, SZ, SE, CH, SY, TW, TJ, TZ, TH, TL, TG, TK, TO, TT, TN, TR, TM, TC, TV, UG, UA, AE, GB, US, UM, UY, UZ, VU, VE, VN, VG, VI, WF, EH, YE, ZM, ZW }


        private string targetUrl;
        private GProxyType targetType;
        private ScrapeType scrapeType;
        private int maxProxyWait;

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

        //https://proxyscrape.com/free-proxy-list
        public GProxyScraper FromProxyScrape(GProxyType proxyType, int timeout, CountryCode countryCode, bool ssl = false, bool anon = false)
        {
            if (timeout == -1) throw new ArgumentException("Invalid timeout.");
            maxProxyWait = timeout;
            //if (string.IsNullOrEmpty(country)) throw new ArgumentException("Invalid country.");
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

            requestUrl += $@"&timeout={timeout}";
            if (countryCode != CountryCode.Any) requestUrl += $@"&country={countryCode.ToString()}";
            
            
            if (proxyType.Equals(GProxyType.Http))
            {
                if (ssl) requestUrl += "&ssl=1";
                if (anon) requestUrl += "&anonymity=elite";
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

        public enum GeonodeAnonType
        {
            None,
            Elite,
            Anonymous,
            //Transparent
        }
        
        //https://geonode.com/free-proxy-list
        public GProxyScraper FromGeonode(GProxyType proxyType, int timeout, CountryCode countryCode, int limit = 500, int page = 1, GeonodeSpeed speed = GeonodeSpeed.Any, 
            string uptime = "any", string lastCheck = "any", GeonodeAnonType anonType = GeonodeAnonType.None)
        {
            if (timeout == -1) throw new ArgumentNullException(nameof(timeout));
            maxProxyWait = timeout;
            if (limit == -1) throw new ArgumentNullException(nameof(limit));
            if (page == -1) throw new ArgumentNullException(nameof(page));
            if (string.IsNullOrEmpty(uptime)) throw new ArgumentNullException(nameof(uptime));
            if (string.IsNullOrEmpty(lastCheck)) throw new ArgumentNullException(nameof(lastCheck));

            var baseUrl = "https://proxylist.geonode.com/api/proxy-list?";
            baseUrl += $@"limit={limit}&page={page}&sort_by=lastChecked&sort_type=desc"; // sort_by and sort_type are hardcoded for the best lol

            if (uptime != "any") baseUrl += $@"&filterUpTime={uptime}";
            if (lastCheck != "any") baseUrl += $@"&filterLastChecked={lastCheck};";
            
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

            if (anonType != GeonodeAnonType.None && proxyType.Equals(GProxyType.Http))
            {
                baseUrl += "&anonymityLevel=";
                switch (anonType)
                {
                    case GeonodeAnonType.Anonymous:
                        baseUrl += "anonymous";
                        break;
                    case GeonodeAnonType.Elite:
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
                        var maxTime = maxProxyWait * 1000; // filtering response time has to be done here since it's not provided in the "api"
                        results.AddRange(from gp in json.Proxies where gp.ResponseTime <= maxTime select new GProxy(gp.Ip, gp.Port, targetType.ToString()));
                    }

                    return results;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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