using FluentValidation;
using Telebill.Dto.Coding;

namespace Telebill.Validations.Coding;

public class UpdateChargeLineModifiersDtoValidator : AbstractValidator<UpdateChargeLineModifiersDto>
{
    public UpdateChargeLineModifiersDtoValidator()
    {
        RuleFor(x => x.Modifiers)
            .NotNull()
            .Must(m => m.Count <= 4)
            .WithMessage("Maximum 4 modifiers per charge line (CMS rule).");

        RuleForEach(x => x.Modifiers)
            .NotEmpty()
            .MaximumLength(10)
            .When(x => x.Modifiers != null);
    }
}
