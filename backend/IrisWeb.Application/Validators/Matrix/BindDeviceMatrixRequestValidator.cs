using FluentValidation;
using IrisWeb.Application.Communication.Matrix.Requests;
using IrisWeb.Domain.Errors;

namespace IrisWeb.Application.Validators.Matrix;

public sealed class BindDeviceMatrixRequestValidator : AbstractValidator<BindDeviceMatrixRequest>
{
    public BindDeviceMatrixRequestValidator()
    {
        RuleFor(x => x.Line)
            .NotEmpty().WithMessage(MatrixErrors.LineRequired)
            .MaximumLength(MatrixErrors.MaxLineLength)
            .WithMessage(MatrixErrors.FieldTooLong("Linha", MatrixErrors.MaxLineLength));

        RuleFor(x => x.Product)
            .NotEmpty().WithMessage(MatrixErrors.ProductRequired)
            .MaximumLength(MatrixErrors.MaxProductLength)
            .WithMessage(MatrixErrors.FieldTooLong("Produto", MatrixErrors.MaxProductLength));

        RuleFor(x => x.Shift)
            .NotEmpty().WithMessage(MatrixErrors.ShiftRequired)
            .MaximumLength(MatrixErrors.MaxShiftLength)
            .WithMessage(MatrixErrors.FieldTooLong("Turno", MatrixErrors.MaxShiftLength));

        RuleFor(x => x.StationId).GreaterThan(0).WithMessage(MatrixErrors.StationRequired);
    }
}
