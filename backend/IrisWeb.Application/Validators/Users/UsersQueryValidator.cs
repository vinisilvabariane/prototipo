using FluentValidation;
using IrisWeb.Application.Communication.Users.Requests;
using IrisWeb.Domain.Errors;

namespace IrisWeb.Application.Validators.Users;

public sealed class UsersQueryValidator : AbstractValidator<UsersQuery>
{
    public UsersQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(0).WithMessage(PagingErrors.PageNegative);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, PagingErrors.MaxPageSize)
            .WithMessage(PagingErrors.PageSizeOutOfRange);

        RuleFor(x => x.Search)
            .Must(search => search!.Trim().Length <= PagingErrors.MaxSearchLength)
            .When(x => x.Search is not null)
            .WithMessage(PagingErrors.SearchTooLong);
    }
}
