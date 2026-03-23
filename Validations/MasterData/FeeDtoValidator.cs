using FluentValidation;
using Telebill.Dto.MasterData;
using Telebill.Validations;

namespace Telebill.Validations.MasterData;

public class FeeDtoValidator : AbstractValidator<FeeDTO>
{
    public FeeDtoValidator()
    {
        RuleFor(x => x.PlanId)
            .Must(id => id.HasValue && id.Value > 0)
            .WithMessage("PlanId is required and must be greater than zero.");

        RuleFor(x => x.CptHcpcs)
            .NotEmpty()
            .MaximumLength(10);

        RuleFor(x => x.ModifierCombo)
            .MaximumLength(50)
            .When(x => x.ModifierCombo != null);

        RuleFor(x => x.AllowedAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.AllowedAmount.HasValue);

        RuleFor(x => x)
            .Must(f => !f.EffectiveFrom.HasValue || !f.EffectiveTo.HasValue || f.EffectiveTo.Value >= f.EffectiveFrom.Value)
            .WithMessage("EffectiveTo must be on or after EffectiveFrom.");

        RuleFor(x => x.Status)
            .Must(EnumValidationHelpers.IsValidEnumName<MasterEntityStatus>)
            .WithMessage(_ =>
                $"Status must be one of: {EnumValidationHelpers.AllowedNames<MasterEntityStatus>()}.")
            .When(x => !string.IsNullOrWhiteSpace(x.Status));
    }
}
