using Linger.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace Linger.Client
{
    public partial class MainForm : Form
    {
        private readonly BlazorWebView blazorWebView;

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

            // 添加HTTP客户端
            services.AddHttpClient("LingerAPI", client =>
            {
                // 确保这里的URL与API实际运行的端口一致
                client.BaseAddress = new Uri("https://localhost:7001/");
            });

            // 添加服务
            services.AddScoped<AuthService>();
            services.AddScoped<ApiService>();

            // 添加状态管理
            services.AddSingleton<AppState>();

            // 添加认证相关服务
            services.AddAuthorizationCore();
            services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

            blazorWebView.Services = services.BuildServiceProvider();
            blazorWebView.RootComponents.Add<App>("#app");

            Controls.Add(blazorWebView);
        }
    }
}
