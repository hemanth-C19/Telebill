using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Telebill.Dto.Auth;
using Telebill.Dto.IdentityAccess;
using Telebill.Repositories.Auth;
using Telebill.Services.IdentityAccess;

namespace Telebill.Services.Auth;

public class AuthService(
    IAuthRepository authRepository,
    IConfiguration config,
    IAuditService auditService) : IAuthService
{
    public async Task<LoginResponseDto?> LoginAsync(LoginDto loginDto)
    {
        var user = await authRepository.LoginAsync(loginDto.Email.ToLower(), loginDto.Password, loginDto.Role);
        if (user == null)
            return null;

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var tokenSecret = config["Token"]
            ?? throw new InvalidOperationException("Token is not configured in appsettings.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var expires = DateTime.UtcNow.AddDays(1);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var finalToken = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(finalToken);

        try
        {
            await auditService.AddAsync(new AuditLogDTO
            {
                UserId = user.UserId,
                Action = "LOGIN",
                Resource = "POST /api/AuthController-Module/login",
                Timestamp = DateTime.UtcNow,
                Metadata = JsonSerializer.Serialize(new { Email = user.Email, Role = user.Role })
            });
        }
        catch
        {
            Console.WriteLine("Issue in AuthService");
        }

        return new LoginResponseDto
        {
            Token = tokenString,
            ExpiresAt = expires,
            UserId = user.UserId,
            Email = user.Email,
            Name = user.Name,
            Role = user.Role
        };
    }

    public void Logout()
    {
        authRepository.Logout();
    }
}
