using FluentValidation;
using MeOne.Application.Communication.Suppliers.Requests;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.Suppliers;

public sealed class SuppliersQueryValidator : AbstractValidator<SuppliersQuery>
{
    public SuppliersQueryValidator()
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
