using FluentValidation;
using IrisWeb.Application.Communication.Matrix.Requests;
using IrisWeb.Domain.Errors;

namespace IrisWeb.Application.Validators.Matrix;

public sealed class MatrixProductsQueryValidator : AbstractValidator<MatrixProductsQuery>
{
    public MatrixProductsQueryValidator()
    {
        RuleFor(x => x.Line)
            .NotEmpty().WithMessage(MatrixErrors.LineRequired)
            .MaximumLength(MatrixErrors.MaxLineLength)
            .WithMessage(MatrixErrors.FieldTooLong("Linha", MatrixErrors.MaxLineLength));
    }
}
