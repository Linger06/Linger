using Linger.API.Models;

namespace Linger.API.Services;

public class UserService
{
    // 模拟数据库中的用户集合
    private static readonly List<UserInfo> s_users =
    [
        new UserInfo
        {
            Id = "1",
            Name = "测试用户",
            Email = "test@example.com",
            AvatarUrl = "/images/avatars/default.png"
        }
    ];

    // 获取单个用户
    public UserInfo? GetUser(string id)
    {
        return s_users.FirstOrDefault(u => u.Id == id);
    }

    // 创建新用户
    public UserInfo CreateUser(UserCreateRequest request)
    {
        // 实际应用中应该进行密码哈希等安全处理
        var newUser = new UserInfo
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = request.Name,
            Email = request.Email,
            AvatarUrl = "/images/avatars/default.png"
        };

        s_users.Add(newUser);
        return newUser;
    }

    // 更新用户头像
    public string UpdateAvatar(string userId, string filename)
    {
        var user = s_users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
            return string.Empty;

        // 构建头像URL (在实际应用中，这应该是服务器上存储的路径)
        string avatarUrl = $"/images/avatars/{userId}/{filename}";
        user.AvatarUrl = avatarUrl;

        return avatarUrl;
    }

    // 更新用户信息
    public UserInfo? UpdateUserInfo(UserUpdateRequest request)
    {
        var user = s_users.FirstOrDefault(u => u.Id == request.Id);
        if (user == null)
            return null;

        // 更新用户信息
        user.Name = request.Name;
        user.Email = request.Email;

        return user;
    }
}
