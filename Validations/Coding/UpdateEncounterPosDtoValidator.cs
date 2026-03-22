using FluentValidation;
using Telebill.Dto.Coding;
using Telebill.Validations.MasterData;

namespace Telebill.Validations.Coding;

public class UpdateEncounterPosDtoValidator : AbstractValidator<UpdateEncounterPosDto>
{
    public UpdateEncounterPosDtoValidator()
    {
        RuleFor(x => x.Pos)
            .NotEmpty()
            .Must(TelehealthPosCodes.IsValid)
            .WithMessage(_ =>
                $"POS must be {TelehealthPosCodes.OtherThanPatientHome} or {TelehealthPosCodes.PatientHome}.");
    }
}
