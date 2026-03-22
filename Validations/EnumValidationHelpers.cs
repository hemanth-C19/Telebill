namespace Telebill.Validations;

/// <summary>Shared string→enum checks for FluentValidation rules (DTOs stay string for JSON flexibility).</summary>
public static class EnumValidationHelpers
{
    public static bool IsValidEnumName<TEnum>(string? value) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value)) return true;
        return Enum.TryParse<TEnum>(value.Trim(), ignoreCase: true, out _);
    }

    public static string AllowedNames<TEnum>() where TEnum : struct, Enum =>
        string.Join(", ", Enum.GetNames<TEnum>());
}
