using Telebill.Models;

namespace Telebill.Repositories.Auth;

public interface IAuthRepository
{
    /// <summary>Validates credentials. User table has no password column yet — only non-empty password is required; add hash verification when you store passwords.</summary>
    Task<User?> LoginAsync(string email, string password);

    void Logout();
}