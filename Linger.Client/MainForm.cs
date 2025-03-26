using Linger.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace Linger.Client
{
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

            // 添加HTTP客户端
            services.AddHttpClient("LingerAPI", client =>
            {
                // 确保这里的URL与API实际运行的端口一致
                client.BaseAddress = new Uri("http://localhost:5258/");
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
            blazorWebView.WebView.CoreWebView2InitializationCompleted += (s, e) =>
            {
                blazorWebView.WebView.CoreWebView2.AddHostObjectToScript("winFormHost", new WinFormInterop(this));
            };

            Controls.Add(blazorWebView);

            RegisterHotKey();
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
}
