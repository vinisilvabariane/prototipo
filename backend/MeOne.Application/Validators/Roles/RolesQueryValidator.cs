using FluentValidation;
using MeOne.Application.Communication.Roles.Requests;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.Roles;

public sealed class RolesQueryValidator : AbstractValidator<RolesQuery>
{
    public RolesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(0).WithMessage(PagingErrors.PageNegative);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, PagingErrors.MaxPageSize)
            .WithMessage(PagingErrors.PageSizeOutOfRange);
    }
}
