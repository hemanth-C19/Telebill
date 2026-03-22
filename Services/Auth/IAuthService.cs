using Telebill.Dto.Auth;

namespace Telebill.Services.Auth;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginDto loginDto);

    void Logout();
}
