using System;

namespace Linger.Client.Services
{
    /// <summary>
    /// 应用状态管理类，用于存储跨组件的应用状态
    /// </summary>
    public class AppState
    {
        private bool _isLoggedIn;
        private string _username = string.Empty;
        private string _token = string.Empty;

        /// <summary>
        /// 用户的JWT认证令牌
        /// </summary>
        public string? Token 
        { 
            get => _token; 
            set 
            { 
                _token = value; 
                NotifyStateChanged(); 
            } 
        }
        
        /// <summary>
        /// 用户是否已登录
        /// </summary>
        public bool IsAuthenticated => !string.IsNullOrEmpty(Token);

        public bool IsLoggedIn 
        { 
            get => _isLoggedIn; 
            set 
            { 
                _isLoggedIn = value; 
                NotifyStateChanged(); 
            } 
        }
        
        public string Username 
        { 
            get => _username; 
            set 
            { 
                _username = value; 
                NotifyStateChanged(); 
            } 
        }

        public event Action? OnChange;

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
