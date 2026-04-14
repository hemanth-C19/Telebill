using FluentValidation;
using Telebill.Dto.Coding;

namespace Telebill.Validations.Coding;

public class ApplyCodingLockDtoValidator : AbstractValidator<ApplyCodingLockDto>
{
    public ApplyCodingLockDtoValidator()
    {
        RuleFor(x => x.EncounterId)
            .GreaterThan(0);

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => x.Notes != null);
    }
}
