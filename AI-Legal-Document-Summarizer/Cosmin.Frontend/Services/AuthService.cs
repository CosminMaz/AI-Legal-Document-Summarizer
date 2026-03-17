using System;

namespace Cosmin.Frontend.Services
{
    public class AuthService
    {
        public bool IsLoggedIn { get; private set; }
        public Guid? UserId { get; private set; }
        public string? Username { get; private set; }
        public event Action OnChange;

        public void Login(Guid userId, string username)
        {
            IsLoggedIn = true;
            UserId = userId;
            Username = username;
            NotifyStateChanged();
        }

        public void Logout()
        {
            IsLoggedIn = false;
            UserId = null;
            Username = null;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
