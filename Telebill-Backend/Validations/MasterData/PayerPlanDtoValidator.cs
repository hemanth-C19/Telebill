using FluentValidation;
using Telebill.Dto.MasterData;

namespace Telebill.Validations.MasterData;

public class PayerPlanDtoValidator : AbstractValidator<AddPayerPlanDTO>
{
    public PayerPlanDtoValidator()
    {
        RuleFor(x => x.PlanName)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.PayerId)
            .Must(id => id.HasValue && id.Value > 0)
            .WithMessage("PayerId is required and must be greater than zero.");

        RuleFor(x => x.NetworkType)
            .Must(v => Enum.TryParse<PlanNetworkType>(v, ignoreCase: true, out _))
            .WithMessage($"NetworkType must be one of: {string.Join(", ", Enum.GetNames<PlanNetworkType>())}.")
            .When(x => !string.IsNullOrWhiteSpace(x.NetworkType));

        RuleFor(x => x.Posdefault)
            .Must(p => string.IsNullOrWhiteSpace(p) || TelehealthPosCodes.IsValid(p))
            .WithMessage($"POSDefault must be {TelehealthPosCodes.OtherThanPatientHome} or {TelehealthPosCodes.PatientHome}.")
            .When(x => x.Posdefault != null);

        RuleFor(x => x.TelehealthModifiersJson)
            .MaximumLength(400)
            .When(x => x.TelehealthModifiersJson != null);

        RuleFor(x => x.Status)
            .Must(v => Enum.TryParse<MasterEntityStatus>(v, ignoreCase: true, out _))
            .WithMessage($"Status must be one of: {string.Join(", ", Enum.GetNames<MasterEntityStatus>())}.")
            .When(x => !string.IsNullOrWhiteSpace(x.Status));
    }
}
