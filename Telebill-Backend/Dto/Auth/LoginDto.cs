namespace Telebill.Dto.Auth;

public class LoginDto
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string Role {get; set;} = null!;
}
