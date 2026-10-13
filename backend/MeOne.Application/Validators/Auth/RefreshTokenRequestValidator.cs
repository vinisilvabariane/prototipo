using FluentValidation;
using MeOne.Application.Communication.Auth.Requests;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.Auth;

public sealed class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage(AuthErrors.RefreshTokenRequired)
            .MaximumLength(AuthErrors.MaxRefreshTokenLength)
            .WithMessage(AuthErrors.FieldTooLong("O refresh token", AuthErrors.MaxRefreshTokenLength));
    }
}
