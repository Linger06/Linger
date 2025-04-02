namespace Linger.API.Models;

public class UserInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
}

public class UserCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class ApiResult<T>
{
    public bool IsSuccess => string.IsNullOrEmpty(ErrorMsg);
    public string ErrorMsg { get; set; } = string.Empty;
    public T? Data { get; set; }
}
