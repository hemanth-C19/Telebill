using System.Text.RegularExpressions;
using FluentValidation;
using Telebill.Dto.MasterData;
using Telebill.Validations;

namespace Telebill.Validations.MasterData;

public class CreateUpdateProviderDtoValidator : AbstractValidator<CreateUpdateProviderDTO>
{
    private static readonly Regex NpiDigits = new("^[0-9]{10}$", RegexOptions.Compiled);

    public CreateUpdateProviderDtoValidator()
    {
        RuleFor(x => x.ProviderName)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.ProviderNpi)
            .NotEmpty()
            .Must(n => n != null && NpiDigits.IsMatch(n.Trim()))
            .WithMessage("Provider NPI must be exactly 10 digits.");

        RuleFor(x => x.ProviderTaxonomy)
            .MaximumLength(100)
            .When(x => x.ProviderTaxonomy != null);

        RuleFor(x => x.ProviderContact)
            .MaximumLength(100)
            .When(x => x.ProviderContact != null);

        RuleFor(x => x.ProviderStatus)
            .Must(EnumValidationHelpers.IsValidEnumName<MasterEntityStatus>)
            .WithMessage(_ =>
                $"ProviderStatus must be one of: {EnumValidationHelpers.AllowedNames<MasterEntityStatus>()}.")
            .When(x => !string.IsNullOrWhiteSpace(x.ProviderStatus));
    }
}
