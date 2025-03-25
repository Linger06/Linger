using System.Net.Http.Json;
using Linger.Client.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace Linger.Client.Services
{
    public class AuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AppState _appState;
        private readonly AuthenticationStateProvider _authStateProvider;

        public AuthService(IHttpClientFactory httpClientFactory, AppState appState, AuthenticationStateProvider authStateProvider)
        {
            _httpClientFactory = httpClientFactory;
            _appState = appState;
            _authStateProvider = authStateProvider;
        }

        public async Task<bool> Login(LoginRequest loginRequest)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("LingerAPI");
                var response = await client.PostAsJsonAsync("api/auth/login", loginRequest);

                Console.WriteLine($"登录响应状态码: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.Token))
                    {
                        Console.WriteLine($"获取到令牌: {loginResponse.Token[..10]}...");
                        _appState.Token = loginResponse.Token;
                        _appState.Username = loginRequest.Username;
                        _appState.IsLoggedIn = true;
                        // 触发状态变化，通知AuthStateProvider
                        ((CustomAuthStateProvider)_authStateProvider).NotifyAuthenticationStateChanged();
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("令牌为空或登录响应为null");
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"登录失败: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"登录异常: {ex.Message}");
            }

            return false;
        }

        public Task<bool> Logout()
        {
            _appState.Token = string.Empty;
            _appState.Username = string.Empty;
            _appState.IsLoggedIn = false;
            return Task.FromResult(true);
        }

        public bool IsAuthenticated => _appState.IsLoggedIn;
    }
}
