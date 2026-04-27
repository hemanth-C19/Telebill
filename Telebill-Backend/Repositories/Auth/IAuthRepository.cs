using Telebill.Models;

namespace Telebill.Repositories.Auth;

public interface IAuthRepository
{
    Task<User?> LoginAsync(string email, string role);

    void Logout();
}