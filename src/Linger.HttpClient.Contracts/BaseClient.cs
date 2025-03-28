using System.Net;
using System.Text;
using System.Text.Json;
using Linger.Extensions;
using Linger.Extensions.Core;
#if NETFRAMEWORK
using System.Net.Http;
#endif

namespace Linger.HttpClient.Contracts;

public abstract class BaseClient : IHttpClient
{

    /// <summary>
    ///     使用Get方法调用api
    /// </summary>
    /// <param name="url">调用地址</param>
    /// <param name="queryParams"></param>
    /// <param name="timeout">超时时间,单位秒</param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public virtual async Task<ApiResult<T>> CallApi<T>(string url, object? queryParams = null, int? timeout = null, CancellationToken cancellationToken = default) // where T : class
    {
        return await CallApi<T>(url, HttpMethodEnum.Get, null, queryParams, timeout, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="url">调用地址</param>
    /// <param name="method">调用方式</param>
    /// <param name="postData">Post Json Data,提交的object,会被转成HttpContent提交</param>
    /// <param name="queryParams">查询参数</param>
    /// <param name="timeout">超时时间,单位秒</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<ApiResult<T>> CallApi<T>(string url, HttpMethodEnum method, object? postData = null, object? queryParams = null, int? timeout = null, CancellationToken cancellationToken = default) //where T : class
    {
        HttpContent? content = null;

        if (postData == null)
        {
            return await CallApi<T>(url, method, content, queryParams, timeout, cancellationToken);
        }

        if (postData is IDictionary<string, string> dictionary)
        {
            var content2 = new MultipartFormDataContent();
            //填充表单数据
            if (!(dictionary.IsNull() || dictionary.Count == 0))
            {
                foreach (var key in dictionary.Keys)
                {
                    content2.Add(new StringContent(dictionary[key]), key);
                }
            }

            content = content2;
        }
        else
        {
            var json = JsonSerializer.Serialize(postData, ExtensionMethodSetting.DefaultPostJsonOption);
            content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        return await CallApi<T>(url, method, content, queryParams, timeout, cancellationToken);
    }

    public virtual async Task<ApiResult<T>> CallApi<T>(string url, HttpMethodEnum method, IDictionary<string, string>? postData, int? timeout = null, CancellationToken cancellationToken = default) //where T : class
    {
        HttpContent? content = null;
        //填充表单数据
        if (postData == null || postData.Count == 0)
        {
            return await CallApi<T>(url, method, content, null, timeout, cancellationToken);
        }

        var paras = postData.Select(data => new KeyValuePair<string, string>(data.Key, data.Value)).ToList();

        if (paras.Count != 0)
        {
            url = url.AppendQuery(paras);
        }

        content = new FormUrlEncodedContent(paras);

        return await CallApi<T>(url, method, content, null, timeout, cancellationToken);
    }

    /// <summary>
    ///     提交表单
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="url"></param>
    /// <param name="method"></param>
    /// <param name="postData"></param>
    /// <param name="fileData"></param>
    /// <param name="filename"></param>
    /// <param name="timeout"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<ApiResult<T>> CallApi<T>(string url, HttpMethodEnum method, IDictionary<string, string>? postData, byte[] fileData, string filename, int? timeout = null, CancellationToken cancellationToken = default) //where T : class
    {
        var content = new MultipartFormDataContent();
        //填充表单数据
        if (!(postData == null || postData.Count == 0))
        {
            foreach (var key in postData.Keys)
            {
                content.Add(new StringContent(postData[key]), key);
            }
        }

        content.Add(new ByteArrayContent(fileData), "File", filename);

        return await CallApi<T>(url, method, content, null, timeout, cancellationToken).ConfigureAwait(false);
    }

    public abstract Task<ApiResult<T>> CallApi<T>(string url, HttpMethodEnum method, HttpContent? content = null, object? queryParams = null, int? timeout = null, CancellationToken cancellationToken = default); //where T : class;
    public abstract void SetToken(string token);
    protected static async Task<ApiResult<T>> HandleResponseMessage<T>(HttpResponseMessage res) //where T : class
    {
        var rv = new ApiResult<T> { StatusCode = res.StatusCode };

        if (res.IsSuccessStatusCode)
        {
            Type type = typeof(T);
            if (type == typeof(byte[]))
            {
                var responseBytes = await res.Content.ReadAsByteArrayAsync();

                if (responseBytes is not T bytes)
                {
                    throw new NullReferenceException(nameof(bytes));
                }

                rv.Data = bytes;
            }
            else
            {
                var responseTxt = await res.Content.ReadAsStringAsync();
                if (type == typeof(string))
                {
                    if (responseTxt is not T txt)
                    {
                        throw new NullReferenceException(nameof(txt));
                    }

                    rv.Data = txt;
                }
                else
                {
                    if (responseTxt.IsNull())
                    {
                        rv.Data = default!;
                    }
                    else
                    {
                        T? response = responseTxt.Deserialize<T>(ExtensionMethodSetting.DefaultJsonSerializerOptions);
                        if (response.IsNull())
                        {
                            rv.Data = default!;
                        }
                        else
                        {
                            rv.Data = response;
                        }
                    }
                }
            }
        }
        else
        {
            if (res.StatusCode == HttpStatusCode.Unauthorized)
            {
                return rv;
            }

            var responseTxt = await res.Content.ReadAsStringAsync();

            try
            {
                rv.Errors = responseTxt.Deserialize<ErrorObj>(ExtensionMethodSetting.DefaultJsonSerializerOptions);
            }
            catch { rv.ErrorMsg = responseTxt; }
        }

        return rv;
    }
}