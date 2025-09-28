using System.Net;
using Linger.Blazor.Services;
using Linger.Client.Services;
using Linger.HttpClient.Contracts.Core;
using Linger.HttpClient.Standard;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using MudBlazor.Services;
using Polly;

namespace Linger.Client;

public partial class MainForm : Form
{
    public readonly BlazorWebView blazorWebView;
    private FormBorderStyle _previousBorderStyle;
    private Rectangle _previousBounds;
    private bool _wasMaximized;
    private AppState _appState;
    private TokenRefreshHandler _tokenRefreshHandler;

    public bool IsFullScreen => FormBorderStyle == FormBorderStyle.None;

    public MainForm()
    {
        InitializeComponent();

        Text = "Linger - Blazor in WinForms";
        Size = new System.Drawing.Size(1280, 720);

        blazorWebView = new BlazorWebView()
        {
            Dock = DockStyle.Fill,
            HostPage = "wwwroot/index.html"
        };

        var services = new ServiceCollection();
        services.AddWindowsFormsBlazorWebView();
        services.AddMudServices();

        // 注册AppState
        services.AddSingleton<AppState>();

        // 注册令牌刷新处理器
        services.AddSingleton<TokenRefreshHandler>();

        // 注册 WinForms 全屏服务实现
        services.AddScoped<IFullScreenService, WinFormFullScreenService>();

        // 使用HttpClientFactory注册IHttpClient，并结合弹性策略
        services.AddHttpClient<IHttpClient, StandardHttpClient>(client =>
        {
            client.BaseAddress = new Uri("http://localhost:5258/");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("User-Agent", "Linger.Client");
            client.Timeout = TimeSpan.FromSeconds(30); // 设置超时时间
        })
        .AddTypedClient<IHttpClient>((httpClient, serviceProvider) =>
        {
            var standardClient = new StandardHttpClient(httpClient);

            // 获取AppState用于设置令牌
            var appState = serviceProvider.GetRequiredService<AppState>();
            if (!string.IsNullOrEmpty(appState.Token))
            {
                standardClient.SetToken(appState.Token);
            }

            // 订阅Token变化事件
            appState.OnChange += () =>
            {
                if (!string.IsNullOrEmpty(appState.Token))
                {
                    standardClient.SetToken(appState.Token);
                }
            };

            return standardClient;
        })
        .AddResilienceHandler("Default", (builder, context) =>
        {
            builder.AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(2),
                ShouldHandle = args =>
                {
                    if (args.Outcome.Result != null && args.Outcome.Result.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        // 修复错误：更改为 ValueTask.FromResult(true)
                        return ValueTask.FromResult(true);
                    }

                    // 使用默认的重试条件
                    return ValueTask.FromResult(args.Outcome.Result?.StatusCode is
                        HttpStatusCode.RequestTimeout or        // 408
                        HttpStatusCode.BadGateway or           // 502
                        HttpStatusCode.ServiceUnavailable or    // 503
                        HttpStatusCode.GatewayTimeout);        // 504
                }
            });

            // 获取令牌刷新处理器并配置令牌刷新策略
            var tokenRefreshHandler = context.ServiceProvider.GetRequiredService<TokenRefreshHandler>();
            tokenRefreshHandler.ConfigureTokenRefreshResiliencePipeline(builder);
        })
        ;

        // 添加服务
        services.AddScoped<AuthService>();
        services.AddScoped<UserService>(); // 注册UserService

        // 添加认证相关服务
        services.AddAuthorizationCore();
        services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

        blazorWebView.Services = services.BuildServiceProvider();
        blazorWebView.RootComponents.Add<Blazor.App>("#app");
        blazorWebView.WebView.CoreWebView2InitializationCompleted += (_, _) =>
        {
            blazorWebView.WebView.CoreWebView2.AddHostObjectToScript("winFormHost", new WinFormInterop(this));
        };

        Controls.Add(blazorWebView);
        RegisterHotKey();

        // 获取AppState和TokenRefreshHandler的引用
        _appState = blazorWebView.Services.GetRequiredService<AppState>();
        _tokenRefreshHandler = blazorWebView.Services.GetRequiredService<TokenRefreshHandler>();

        // 订阅令牌刷新事件
        _tokenRefreshHandler.TokenRefreshed += (_, newToken) =>
        {
            _appState.Token = newToken;
        };

        _tokenRefreshHandler.TokenRefreshFailed += (_, _) =>
        {
            // 令牌刷新失败，无需额外操作，TokenRefreshHandler中会自动触发RequireRelogin
        };
    }

    private void RegisterHotKey()
    {
        KeyPreview = true;
        KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.F11)
            {
                ToggleFullScreen();
                e.Handled = true;
            }
        };
    }

    public void ToggleFullScreen()
    {
        if (FormBorderStyle != FormBorderStyle.None)
        {
            // Save current state
            _previousBorderStyle = FormBorderStyle;
            _previousBounds = Bounds;
            _wasMaximized = WindowState == FormWindowState.Maximized;
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
        }
        else
        {
            FormBorderStyle = _previousBorderStyle;
            Bounds = _previousBounds;
            WindowState = _wasMaximized ? FormWindowState.Maximized : FormWindowState.Normal;
        }

        blazorWebView.WebView.ExecuteScriptAsync("onFullScreenChanged(" + (IsFullScreen ? "true" : "false") + ");");
    }
}
