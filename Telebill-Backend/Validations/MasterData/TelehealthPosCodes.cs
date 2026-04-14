namespace Telebill.Validations.MasterData;

/// <summary>Telehealth place-of-service values used on encounters and payer plans (02 / 10).</summary>
public static class TelehealthPosCodes
{
    public const string OtherThanPatientHome = "02";

    public const string PatientHome = "10";

    private static readonly HashSet<string> Allowed = new(StringComparer.Ordinal)
    {
        OtherThanPatientHome,
        PatientHome
    };

    public static bool IsValid(string? pos) =>
        !string.IsNullOrWhiteSpace(pos) && Allowed.Contains(pos.Trim());
}
