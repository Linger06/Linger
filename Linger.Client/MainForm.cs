using Linger.Client.Services;
using Linger.HttpClient.Contracts.Core;
using Linger.HttpClient.Standard;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using Polly;
using Polly.Extensions.Http;
using System.Net;

namespace Linger.Client;

public partial class MainForm : Form
{
    public readonly BlazorWebView blazorWebView;
    private FormBorderStyle _previousBorderStyle;
    private Rectangle _previousBounds;
    private bool _wasMaximized;

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

        // 使用HttpClientFactory注册IHttpClient，并结合Polly策略
        services.AddHttpClient<IHttpClient, StandardHttpClient>(client =>
        {
            client.BaseAddress = new Uri("http://localhost:5258/");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("User-Agent", "Linger.Client");
            client.Timeout = TimeSpan.FromSeconds(30); // 设置超时时间
        })
        .AddPolicyHandler(GetRetryPolicy()) // 添加 Polly 重试策略
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
        });

        // 添加服务
        services.AddScoped<AuthService>();
        services.AddScoped<UserService>(); // 注册UserService

        // 添加认证相关服务
        services.AddAuthorizationCore();
        services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

        blazorWebView.Services = services.BuildServiceProvider();
        blazorWebView.RootComponents.Add<App>("#app");
        blazorWebView.WebView.CoreWebView2InitializationCompleted += (s, e) =>
        {
            blazorWebView.WebView.CoreWebView2.AddHostObjectToScript("winFormHost", new WinFormInterop(this));
        };

        Controls.Add(blazorWebView);

        RegisterHotKey();
    }

    // 创建 Polly 重试策略
    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError() // 自动处理网络错误和服务器错误 (5xx)
            .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests) // 也处理 429 响应
            .WaitAndRetryAsync(
                retryCount: 3, // 重试 3 次
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // 指数退避：1s, 2s, 4s
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    Console.WriteLine($"正在进行第 {retryCount} 次重试，等待 {timespan.TotalSeconds} 秒");
                });
    }

    private void RegisterHotKey()
    {
        KeyPreview = true;
        KeyDown += (sender, e) =>
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
