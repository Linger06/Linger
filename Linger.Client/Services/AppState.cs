using System;

namespace Linger.Client.Services
{
    public class AppState
    {
        private bool _isLoggedIn;
        private string _username = string.Empty;
        private string _token = string.Empty;

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
        
        public string Token 
        { 
            get => _token; 
            set 
            { 
                _token = value; 
                NotifyStateChanged(); 
            } 
        }

        public event Action? OnChange;

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
