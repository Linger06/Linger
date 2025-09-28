using System.Text;
using System.Text.Json;
using Linger.HttpClient.Contracts.Core;
using Linger.HttpClient.Contracts.Models;

namespace Linger.Blazor.Services;

public class UserService(IHttpClient httpClient)
{

    // 获取用户信息
    public async Task<ApiResult<UserInfo>> GetUserInfoAsync(string userId)
    {
        return await httpClient.CallApi<UserInfo>($"api/users/{userId}");
    }

    // 创建新用户
    public async Task<ApiResult<UserInfo>> CreateUserAsync(UserCreateModel model)
    {
        return await httpClient.CallApi<UserInfo>("api/users", HttpMethodEnum.Post, model);
    }

    // 上传用户头像
    public async Task<ApiResult<string>> UploadAvatarAsync(string userId, string fileName, byte[] imageData)
    {
        var formData = new Dictionary<string, string>
        {
            { "userId", userId }
        };

        return await httpClient.CallApi<string>(
            "api/users/avatar",
            HttpMethodEnum.Post,
            formData,
            imageData,
            fileName);
    }

    // 更新用户信息
    public async Task<ApiResult<UserInfo>> UpdateUserInfoAsync(UserUpdateRequest request)
    {

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        return await httpClient.CallApi<UserInfo>("api/users", HttpMethodEnum.Put, content: jsonContent);
    }

}

// 用户信息模型
public class UserInfo
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string AvatarUrl { get; set; }
}

// 用户更新请求
public class UserUpdateRequest
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
}

// 创建用户模型
public class UserCreateModel
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}