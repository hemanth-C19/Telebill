using System.Text.RegularExpressions;
using FluentValidation;
using Telebill.Dto.IdentityAccess;

namespace Telebill.Validations.User;

public enum UserRole { Admin, FrontDesk, Provider, Coder, AR }
public enum UserStatus { Active, Inactive }

public class UserAddDtoValidator : AbstractValidator<UserAddDTO>
{
    private static readonly Regex NoDigits = new(@"^[A-Za-z\s'\-]+$", RegexOptions.Compiled);
    private static readonly Regex PhoneDigits = new(@"^\d{10}$", RegexOptions.Compiled);

    public UserAddDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(4).WithMessage("Name must be at least 4 characters.")
            .Must(v => NoDigits.IsMatch(v)).WithMessage("Name must not contain numbers or special characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Name));

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress().WithMessage("Enter a valid email address.");

        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(v => Enum.TryParse<UserRole>(v, ignoreCase: true, out _))
            .WithMessage($"Role must be one of: {string.Join(", ", Enum.GetNames<UserRole>())}.");

        RuleFor(x => x.Phone)
            .Must(v => PhoneDigits.IsMatch(v!))
            .WithMessage("Phone number must be exactly 10 digits.")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Status)
            .Must(v => Enum.TryParse<UserStatus>(v, ignoreCase: true, out _))
            .WithMessage($"Status must be one of: {string.Join(", ", Enum.GetNames<UserStatus>())}.")
            .When(x => !string.IsNullOrWhiteSpace(x.Status));
    }
}

public class UserUpdateDtoValidator : AbstractValidator<UserUpdateDTO>
{
    private static readonly Regex NoDigits = new(@"^[A-Za-z\s'\-]+$", RegexOptions.Compiled);
    private static readonly Regex PhoneDigits = new(@"^\d{10}$", RegexOptions.Compiled);

    public UserUpdateDtoValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("UserId must be a positive integer.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(4).WithMessage("Name must be at least 4 characters.")
            .Must(v => NoDigits.IsMatch(v)).WithMessage("Name must not contain numbers or special characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Name));

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress().WithMessage("Enter a valid email address.");

        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(v => Enum.TryParse<UserRole>(v, ignoreCase: true, out _))
            .WithMessage($"Role must be one of: {string.Join(", ", Enum.GetNames<UserRole>())}.");

        RuleFor(x => x.Phone)
            .Must(v => PhoneDigits.IsMatch(v!))
            .WithMessage("Phone number must be exactly 10 digits.")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Status)
            .Must(v => Enum.TryParse<UserStatus>(v, ignoreCase: true, out _))
            .WithMessage($"Status must be one of: {string.Join(", ", Enum.GetNames<UserStatus>())}.")
            .When(x => !string.IsNullOrWhiteSpace(x.Status));
    }
}
