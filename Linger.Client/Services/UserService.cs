using Linger.HttpClient.Contracts.Core;
using Linger.HttpClient.Contracts.Models;

namespace Linger.Client.Services;

public class UserService
{
    private readonly IHttpClient _httpClient;

    // 通过依赖注入获取IHttpClient
    public UserService(IHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // 获取用户信息
    public async Task<ApiResult<UserInfo>> GetUserInfoAsync(string userId)
    {
        return await _httpClient.CallApi<UserInfo>($"api/users/{userId}");
    }

    // 创建新用户
    public async Task<ApiResult<UserInfo>> CreateUserAsync(UserCreateModel model)
    {
        return await _httpClient.CallApi<UserInfo>("api/users", HttpMethodEnum.Post, model);
    }

    // 上传用户头像
    public async Task<ApiResult<string>> UploadAvatarAsync(string userId, byte[] imageData)
    {
        var formData = new Dictionary<string, string>
        {
            { "userId", userId }
        };

        return await _httpClient.CallApi<string>(
            "api/users/avatar",
            HttpMethodEnum.Post,
            formData,
            imageData,
            "avatar.jpg");
    }
}

// 用户信息模型
public class UserInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
}

// 创建用户模型
public class UserCreateModel
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}
