using System;
using System.Net;

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
    }
}