# Linger.HttpClient WinForms 示例

该项目展示 WinForms 客户端直接创建 `StandardHttpClient`，并通过 `DelegatingHandler` 自动添加和刷新 JWT。完整实现位于项目源码的 `JwtTokenSession`、`AccessTokenHandler` 和窗体初始化代码中。

## 配置可信源并运行

在仓库根目录打开 PowerShell，把以下两个示例源替换为你信任的 HTTPS 服务，然后在**同一个窗口**启动示例：

```powershell
$env:LINGER_WINFORMS_API_ORIGIN = 'https://localhost:7141'
$env:LINGER_WINFORMS_REFRESH_ORIGIN = 'https://localhost:7242'
dotnet run --project examples/Linger.HttpClient.WinForms/Linger.HttpClient.WinForms.csproj
```

- 两个变量必须分别配置，即使两个服务同源。格式为 `https://主机[:端口]`，末尾可带 `/`；不要包含用户信息、路径、查询串或片段。服务端必须提供受信任的 TLS 证书。缺失或无效的配置会阻止对应客户端发送；修改配置后需重启示例。
- 按示例配置，填入令牌和刷新端点 `https://localhost:7242/refresh`，点击“应用令牌”，再向 `https://localhost:7141/users/me` 发送 GET。请把两个路径换成服务端的实际路由。编辑表单 URL 不会新增或更改可信源。
- 业务管线先核对实际请求 URI，再获取 Bearer 令牌；刷新管线独立核对 POST 的实际目标，再发送令牌对。HTTPS 主机和有效端口必须完全匹配；信任范围包含该源的**所有路径**，不要选择托管不可信应用的源。两条管线都不会跟随重定向（包括 307/308），请直接填写最终 HTTPS 端点。

服务端需要按照 [Linger.AspNetCore.Jwt README](../../src/Linger.AspNetCore.Jwt/README.zh-CN.md) 暴露登录和刷新端点。登录成功后，将返回的 Access Token、Refresh Token、刷新端点和需要调用的完整 API 地址填入窗体。

窗体顶部可以在中文和英文之间切换。切换后会立即刷新界面资源，并将所选区域性的名称作为 `Accept-Language` 请求头应用到 API 请求和 Refresh Token 请求。

发送请求时：

1. `AccessTokenHandler` 从 `JwtTokenSession` 取得有效的 Access Token。
2. Access Token 将在过期前一分钟通过刷新端点更新。
3. `SemaphoreSlim` 保证并发请求只执行一次刷新。
4. 刷新成功后同时保存新的 Access Token 和轮换后的 Refresh Token。
5. 刷新失败时清除当前会话，并要求重新登录。

刷新请求使用独立的 `HttpClient`，不会递归进入 `AccessTokenHandler`。普通 `401` 不会被自动重试，上传流等不可重复请求也不会被重放。
