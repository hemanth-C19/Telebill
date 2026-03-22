using System.Text.RegularExpressions;
using FluentValidation;
using Telebill.Dto.Coding;

namespace Telebill.Validations.Coding;

public class AddDiagnosisDtoValidator : AbstractValidator<AddDiagnosisDto>
{
    // ICD-10-CM style: letter + 2 digits + optional decimal and suffix (permissive)
    private static readonly Regex Icd10Pattern = new(
        @"^[A-Za-z][0-9]{2}(\.[0-9A-Za-z]{1,4})?$",
        RegexOptions.Compiled);

    public AddDiagnosisDtoValidator()
    {
        RuleFor(x => x.EncounterId)
            .GreaterThan(0);

        RuleFor(x => x.ICD10Code)
            .NotEmpty()
            .MaximumLength(12)
            .Must(code => Icd10Pattern.IsMatch(code.Trim()))
            .WithMessage("ICD10Code must look like a valid ICD-10 code (e.g. E11.9, Z00.00).");

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.Sequence)
            .InclusiveBetween(1, 12)
            .When(x => x.Sequence.HasValue);
    }
}
