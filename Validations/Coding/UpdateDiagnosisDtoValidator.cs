using System.Text.RegularExpressions;
using FluentValidation;
using Telebill.Dto.Coding;

namespace Telebill.Validations.Coding;

public class UpdateDiagnosisDtoValidator : AbstractValidator<UpdateDiagnosisDto>
{
    private static readonly Regex Icd10Pattern = new(
        @"^[A-Za-z][0-9]{2}(\.[0-9A-Za-z]{1,4})?$",
        RegexOptions.Compiled);

    public UpdateDiagnosisDtoValidator()
    {
        RuleFor(x => x.ICD10Code)
            .MaximumLength(12)
            .Must(code => string.IsNullOrWhiteSpace(code) || Icd10Pattern.IsMatch(code.Trim()))
            .WithMessage("ICD10Code must look like a valid ICD-10 code (e.g. E11.9, Z00.00).");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => x.Description != null);

        RuleFor(x => x.Sequence)
            .InclusiveBetween(1, 12)
            .When(x => x.Sequence.HasValue);
    }
}
