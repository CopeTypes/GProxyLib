using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GProxyLib.api;

public class GProxyTester
{
    private readonly List<GProxy> _proxies;
    private readonly string _testUrl;
    private readonly int _threads;
    private readonly int _timeout;
    
    public GProxyTester(List<GProxy> proxies, string testUrl, int threads = 200, int timeout = 5)
    {
        _proxies = proxies ?? throw new ArgumentNullException(nameof(proxies));
        _testUrl = testUrl ?? throw new ArgumentNullException(nameof(testUrl));
        _threads = threads;
        _timeout = timeout;
    }

    public static GProxyTester Create(IEnumerable<GProxy> proxies, string testUrl, int threads = 200, int timeout = 5)
    {
        return new GProxyTester(proxies.ToList(), testUrl, threads, timeout);
    }
    
    public async Task<List<GProxy>> Test()
    {
        var semaphore = new SemaphoreSlim(_threads, _threads);
        var tasks = _proxies.Select(proxy => TestProxyAsync(semaphore, proxy, _timeout)).ToList();
        var results = new List<GProxy>();

        foreach (var task in tasks)
        {
            try
            {
                var result = await task;
                if (result != null) results.Add(result);
            }
            catch (TaskCanceledException)
            {
                
            }
        }

        return results;
    }

    private async Task<GProxy> TestProxyAsync(SemaphoreSlim semaphore, GProxy proxy, int timeout)
    {
        await semaphore.WaitAsync();

        var proxyHandler = new HttpClientHandler
        {
            Proxy = new WebProxy(proxy.Ip, int.Parse(proxy.Port)),
            UseProxy = true
        };

        var testHttpClient = new HttpClient(proxyHandler);
        testHttpClient.Timeout = TimeSpan.FromSeconds(timeout);

        try
        {
            var response = await testHttpClient.GetAsync(_testUrl);
            if (response.IsSuccessStatusCode) return proxy;
        }
        catch (HttpRequestException)
        {
            return null;
        }
        finally { semaphore.Release(); }
        return null;
    }
}
