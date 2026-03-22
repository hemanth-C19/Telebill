using FluentValidation;
using Telebill.Dto.MasterData;
using Telebill.Validations;

namespace Telebill.Validations.MasterData;

public class PayerPlanDtoValidator : AbstractValidator<PayerPlanDTO>
{
    public PayerPlanDtoValidator()
    {
        RuleFor(x => x.PlanName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.PayerId)
            .Must(id => id.HasValue && id.Value > 0)
            .WithMessage("PayerId is required and must be greater than zero.");

        RuleFor(x => x.NetworkType)
            .Must(EnumValidationHelpers.IsValidEnumName<PlanNetworkType>)
            .WithMessage(_ =>
                $"NetworkType must be one of: {EnumValidationHelpers.AllowedNames<PlanNetworkType>()}.")
            .When(x => !string.IsNullOrWhiteSpace(x.NetworkType));

        RuleFor(x => x.Posdefault)
            .Must(p => string.IsNullOrWhiteSpace(p) || TelehealthPosCodes.IsValid(p))
            .WithMessage(_ =>
                $"POSDefault must be {TelehealthPosCodes.OtherThanPatientHome} or {TelehealthPosCodes.PatientHome}.")
            .When(x => x.Posdefault != null);

        RuleFor(x => x.TelehealthModifiersJson)
            .MaximumLength(4000)
            .When(x => x.TelehealthModifiersJson != null);

        RuleFor(x => x.Status)
            .Must(EnumValidationHelpers.IsValidEnumName<MasterEntityStatus>)
            .WithMessage(_ =>
                $"Status must be one of: {EnumValidationHelpers.AllowedNames<MasterEntityStatus>()}.")
            .When(x => !string.IsNullOrWhiteSpace(x.Status));
    }
}
