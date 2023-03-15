using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace GProxyLib.api.client
{
    public class GClient
    {

        private GClientMode _mode;
        private bool _rotateOnRequest;

        private GProxy _proxy;
        private List<GProxy> _proxyDb;

        private HttpClient _client;

        public static GClient Create()
        {
            return new GClient();
        }

        /// <summary>
        /// Creates a new GClient that will use a single proxy.
        /// </summary>
        /// <param name="proxy">The proxy to use</param>
        /// <returns>GClient</returns>
        /// <exception cref="ArgumentNullException">Invalid proxy provided</exception>
        public GClient Single(GProxy proxy)
        {
            if (proxy == null) throw new ArgumentNullException(nameof(proxy));
            SetProxy(proxy);
            _mode = GClientMode.Single;
            return this;
        }

        /// <summary>
        /// Creates a new GClient that will use multiple proxies
        /// </summary>
        /// <param name="proxies">The list of proxies to use</param>
        /// <param name="rotate">If true a new proxy will be used on each request</param>
        /// <returns>GClient</returns>
        /// <exception cref="ArgumentNullException">Invalid proxy list provided</exception>
        public GClient Multi(List<GProxy> proxies, bool rotate = false)
        {
            _proxyDb = proxies ?? throw new ArgumentNullException(nameof(proxies));
            SetProxy(proxies[0]);
            _mode = GClientMode.Multi;
            _rotateOnRequest = rotate;
            return this;
        }

        
        // Methods from HttpClient
        // todo finish impl, proper error handling
        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
            PreFlight();
            if (_mode != GClientMode.Multi)
            {
                var task = await _client.GetAsync(url);
                if (_rotateOnRequest) SetProxy(RandomProxy());
                return task;
            }
            try
            {
                var response = await _client.GetAsync(url);
                if (response.IsSuccessStatusCode) return response;
                _proxyDb.Remove(_proxy);
                SetProxy(RandomProxy());
                return null;
            }
            catch (HttpRequestException)
            { // todo improve error handling , basic logic outline
                _proxyDb.Remove(_proxy);
                SetProxy(RandomProxy());
                return null;
            }
        }

        public Task<Stream> GetStreamAsync(string url)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(url);
            PreFlight();
            if (_mode != GClientMode.Multi)
            {
                var task = _client.GetStreamAsync(url);
                if (_rotateOnRequest) SetProxy(RandomProxy());
                return task;
            }

            return null; // todo impl
        }

        public Task<string> GetStringAsync(string url)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(url);
            PreFlight();
            if (_mode != GClientMode.Multi)
            {
                var task = _client.GetStringAsync(url);
                if (_rotateOnRequest) SetProxy(RandomProxy());
                return task;
            }

            return null; // todo impl
        }

        public Task<byte[]> GetByteArrayAsync(string url)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(url);
            PreFlight();
            if (_mode != GClientMode.Multi)
            {
                var task = _client.GetByteArrayAsync(url);
                if (_rotateOnRequest) SetProxy(RandomProxy());
                return task;
            }

            return null; // todo impl
        }

        
        private void PreFlight()
        {
            if (_client == null)
                throw new ArgumentNullException(nameof(_client), "GClient has not been initialized correctly.");
            
        }
        
        private void SetProxy(GProxy proxy)
        {
            _proxy = proxy;
            _client = new HttpClient(new HttpClientHandler
            {
                Proxy = _proxy.AsWebProxy(),
                UseProxy = true
            });
        }

        private GProxy RandomProxy()
        {
            return _proxyDb[new Random().Next(_proxyDb.Count)];
        }
        
    }
}