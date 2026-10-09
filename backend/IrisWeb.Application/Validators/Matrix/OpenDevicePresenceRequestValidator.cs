using FluentValidation;
using IrisWeb.Application.Communication.Matrix.Requests;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Rules;

namespace IrisWeb.Application.Validators.Matrix;

public sealed class OpenDevicePresenceRequestValidator : AbstractValidator<OpenDevicePresenceRequest>
{
    public OpenDevicePresenceRequestValidator()
    {
        RuleFor(x => x.Registration)
            .NotEmpty().WithMessage(StationErrors.RegistrationRequired)
            .MaximumLength(StationNamingPolicy.MaxRegistrationLength)
            .WithMessage(StationErrors.FieldTooLong("Matrícula", StationNamingPolicy.MaxRegistrationLength));
    }
}
