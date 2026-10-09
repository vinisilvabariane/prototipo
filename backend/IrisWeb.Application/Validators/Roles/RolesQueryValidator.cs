using FluentValidation;
using IrisWeb.Application.Communication.Roles.Requests;
using IrisWeb.Domain.Errors;

namespace IrisWeb.Application.Validators.Roles;

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
