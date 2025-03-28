using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using Linger.Extensions.Core;
using Linger.HttpClient.Contracts;

#if NETFRAMEWORK
using System.Net;
using System.Net.Http;
#endif

namespace Linger.HttpClient;

public class BaseHttpClient : BaseClient
{
    private readonly System.Net.Http.HttpClient _httpClient;

    public BaseHttpClient(string baseUrl)
    {
        _httpClient = new System.Net.Http.HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    public BaseHttpClient(System.Net.Http.HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public override void SetToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public override async Task<ApiResult<T>> CallApi<T>(string url, HttpMethodEnum method, HttpContent? content = null,
        object? queryParams = null, int? timeout = null, CancellationToken cancellationToken = default) //where T : class
    {
        ApiResult<T> rv = new();

        try
        {
            if (string.IsNullOrEmpty(url))
            {
                return rv;
            }

            url = url.AppendQuery("culture=" + Thread.CurrentThread.CurrentUICulture.Name);
            //如果配置了代理，则使用代理
            //设置超时
            if (timeout.HasValue)
            {
                _httpClient.Timeout = new TimeSpan(0, 0, 0, timeout.Value, 0);
            }

            //LogHelper.Info("Http BaseAddress:" + Client.BaseAddress.ToString());
            //LogHelper.Info("Url:" + url);

            HttpMethod httpMethod = method switch
            {
                HttpMethodEnum.Get => HttpMethod.Get,
                HttpMethodEnum.Post => HttpMethod.Post,
                HttpMethodEnum.Put => HttpMethod.Put,
                HttpMethodEnum.Delete => HttpMethod.Delete,
                _ => throw new ArgumentOutOfRangeException(nameof(method), method, null)
            };

#if NETFRAMEWORK
            if (httpMethod == HttpMethod.Get && content != null)
            {
                throw new ProtocolViolationException("Cannot send a content-body with this verb-type.");
            }
#endif

            //var requestUri = new Uri(url);

            //var request = new HttpRequestMessage
            //{
            //    Method = httpMethod,
            //    RequestUri = requestUri,
            //    Content = content
            //};

            var request = new HttpRequestMessage(httpMethod, url)
            {
                Content = content
            };
            ConfiguredTaskAwaitable<HttpResponseMessage> response = _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            HttpResponseMessage res = response.GetAwaiter().GetResult();

            //HttpResponseMessage res = method switch
            //{
            //    HttpMethodEnum.Get => await _httpClient.GetAsync(url),
            //    HttpMethodEnum.Post => await _httpClient.PostAsync(url, content),
            //    HttpMethodEnum.Put => await _httpClient.PutAsync(url, content),
            //    HttpMethodEnum.Delete => await _httpClient.DeleteAsync(url),
            //    _ => throw new ArgumentOutOfRangeException(nameof(method), method, null)
            //};

            rv = await HandleResponseMessage<T>(res);

            return rv;
        }
        catch (Exception ex)
        {
            //ex.WriteLog();
            rv.ErrorMsg = ex.ToString();
            return rv;
        }
    }
}