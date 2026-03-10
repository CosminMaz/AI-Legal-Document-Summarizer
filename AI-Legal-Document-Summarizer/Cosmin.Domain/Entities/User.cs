using Cosmin.Domain.Enums;

namespace Cosmin.Domain.Entities;

public class User
{
    private User() { }

    public static User Create(string u, string e, string p, UserRole r)
    {
        if (string.IsNullOrEmpty(u))
        {
            throw new ArgumentNullException(nameof(u));
        }

        return string.IsNullOrEmpty(e)
            ? throw new ArgumentNullException(nameof(e))
            : new User
            {
                Username = u,
                Email = e,
                Password = p,
                Role = r,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
    }
    
    public Guid Id { private set;  get; } = Guid.NewGuid();
    public string Username { private set; get; }
    public string Email { private set; get; }
    public string Password { private set; get; }
    public UserRole Role { private set; get; }
    public DateTimeOffset CreatedAt { private set; get; }
    public DateTimeOffset UpdatedAt { private set; get; }
}