using FluentValidation;
using Telebill.Dto.Coding;

namespace Telebill.Validations.Coding;

public class UnlockCodingDtoValidator : AbstractValidator<UnlockCodingDto>
{
    public UnlockCodingDtoValidator()
    {
        RuleFor(x => x.EncounterId)
            .GreaterThan(0);

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(2000);
    }
}
