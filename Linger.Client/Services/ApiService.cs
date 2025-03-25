using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Linger.Client.Services
{
    public class ApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AppState _appState;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiService(IHttpClientFactory httpClientFactory, AppState appState)
        {
            _httpClientFactory = httpClientFactory;
            _appState = appState;
            
            // 配置JSON序列化选项
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<T> GetDataAsync<T>(string endpoint)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("LingerAPI");
                
                // 添加JWT令牌到请求头
                if (!string.IsNullOrEmpty(_appState.Token))
                {
                    Console.WriteLine($"发送API请求，携带令牌: {_appState.Token[..10]}...");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _appState.Token);
                }
                else
                {
                    Console.WriteLine("警告: 发送请求时令牌为空");
                }

                var response = await client.GetAsync(endpoint);
                Console.WriteLine($"API响应状态码: {response.StatusCode}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API错误响应: {errorContent}");
                    throw new HttpRequestException($"API请求失败: {response.StatusCode}, {errorContent}");
                }
                
                // 读取响应内容
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API响应内容: {content}");
                
                // 特殊处理简单类型（如果响应是直接的数字、布尔值或字符串）
                if (typeof(T) == typeof(int) && int.TryParse(content, out var intValue))
                {
                    return (T)(object)intValue;
                }
                else if (typeof(T) == typeof(bool) && bool.TryParse(content, out var boolValue))
                {
                    return (T)(object)boolValue;
                }
                else if (typeof(T) == typeof(string))
                {
                    return (T)(object)content.Trim('"'); // 去掉JSON字符串的引号
                }
                
                // 对于复杂类型，使用标准JSON反序列化
                return JsonSerializer.Deserialize<T>(content, _jsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API请求异常: {ex}");
                throw;
            }
        }

        public async Task<T?> PostDataAsync<T>(string endpoint, object data)
        {
            var client = _httpClientFactory.CreateClient("LingerAPI");
            
            // 添加JWT令牌到请求头
            if (!string.IsNullOrEmpty(_appState.Token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _appState.Token);
            }

            var response = await client.PostAsJsonAsync(endpoint, data);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
        }
    }
}
