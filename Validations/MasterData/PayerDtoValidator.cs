using FluentValidation;
using Telebill.Dto.MasterData;
using Telebill.Validations;

namespace Telebill.Validations.MasterData;

public class PayerDtoValidator : AbstractValidator<PayerDTO>
{
    public PayerDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.PayerCode)
            .MaximumLength(50)
            .When(x => x.PayerCode != null);

        RuleFor(x => x.ClearinghouseCode)
            .MaximumLength(50)
            .When(x => x.ClearinghouseCode != null);

        RuleFor(x => x.ContactInfo)
            .MaximumLength(500)
            .When(x => x.ContactInfo != null);

        RuleFor(x => x.Status)
            .Must(EnumValidationHelpers.IsValidEnumName<MasterEntityStatus>)
            .WithMessage(_ =>
                $"Status must be one of: {EnumValidationHelpers.AllowedNames<MasterEntityStatus>()}.")
            .When(x => !string.IsNullOrWhiteSpace(x.Status));
    }
}
