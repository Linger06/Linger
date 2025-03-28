using Flurl;
using Flurl.Http;
using Linger.HttpClient.Contracts;
#if NETFRAMEWORK
using System.Net.Http;
#endif

namespace Linger.HttpClient.Flurl;

public class FlurlHttpClient : BaseClient
{
    private readonly IFlurlClient _flurlClient;

    public FlurlHttpClient(string baseUrl)
    {
        _flurlClient = new FlurlClient(baseUrl);
    }

    public FlurlHttpClient(System.Net.Http.HttpClient httpClient)
    {
        _flurlClient = new FlurlClient(httpClient);
    }

    public FlurlHttpClient(IFlurlClient flurlClient)
    {
        _flurlClient = flurlClient ?? throw new ArgumentNullException(nameof(flurlClient));
    }

    public override void SetToken(string token)
    {
        _ = _flurlClient.WithOAuthBearerToken(token);
    }

    /// <summary>
    ///     最终都会调用此Api
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="url">调用地址</param>
    /// <param name="method">调用方式</param>
    /// <param name="content">HttpContent</param>
    /// <param name="queryParams">查询参数</param>
    /// <param name="timeout">超时时间,单位秒</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task<ApiResult<T>> CallApi<T>(string url, HttpMethodEnum method, HttpContent? content = null,
        object? queryParams = null, int? timeout = null, CancellationToken cancellationToken = default) //where T : class
    {
        ApiResult<T> rv = new();
        if (string.IsNullOrEmpty(url))
        {
            return rv;
        }

        //url = url.AppendQuery("culture=" + Thread.CurrentThread.CurrentUICulture.Name);

        IFlurlRequest? flurlRequest = _flurlClient.Request();

        //flurlClient.Configure(settings =>
        //{
        //    // keeps logging & error handling out of SimpleCastClient
        //    settings.BeforeCall = call => LogHelper.Warning($"Calling {call.Request.Url}");
        //    settings.AfterCall = call => LogHelper.Warning($"Calling Finished {call.Request.Url}");
        //    settings.OnError = call => LogHelper.Error($"Call to SimpleCast failed: {call.Exception}");
        //});

        if (timeout.HasValue)
        {
            flurlRequest = flurlRequest.WithTimeout(timeout.Value);
        }

        var baseUrl = _flurlClient.BaseUrl;
        var baseUrlAddPath = Url.Combine(baseUrl, url);
        var df = new Url(baseUrlAddPath);
        QueryParamCollection? queryParamCollection = df.QueryParams;

        if (queryParamCollection != null)
        {
            flurlRequest = flurlRequest.SetQueryParams(queryParamCollection);
        }

        var query = df.Query;

        url = url.Replace($"?{query}", string.Empty);

        flurlRequest = flurlRequest.AppendPathSegment(url).AllowAnyHttpStatus();

        flurlRequest = flurlRequest.SetQueryParams(new { culture = Thread.CurrentThread.CurrentUICulture.Name });

        if (queryParams != null)
        {
            flurlRequest = flurlRequest.SetQueryParams(queryParams);
        }

        IFlurlResponse? flurlResponse = method switch
        {
            HttpMethodEnum.Get => await flurlRequest.GetAsync(cancellationToken: cancellationToken),
            HttpMethodEnum.Post => await flurlRequest.PostAsync(content, cancellationToken: cancellationToken),
            HttpMethodEnum.Put => await flurlRequest.PutAsync(content, cancellationToken: cancellationToken),
            HttpMethodEnum.Delete => await flurlRequest.DeleteAsync(cancellationToken: cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(method), method, null)
        };

        HttpResponseMessage? res = flurlResponse.ResponseMessage;

        rv = await HandleResponseMessage<T>(res);

        return rv;
    }
}