using FluentValidation;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.Production;

public sealed class SaveProductionShiftRequestValidator : AbstractValidator<SaveProductionShiftRequest>
{
    public SaveProductionShiftRequestValidator()
    {
        RuleFor(x => x.Name).RequiredText(ProductionErrors.ShiftNameRequired, "O nome do turno", ProductionErrors.MaxShiftNameLength);
        RuleFor(x => x).Must(x => x.StartTime != x.EndTime).WithMessage(ProductionErrors.ShiftTimesEqual);
    }
}
