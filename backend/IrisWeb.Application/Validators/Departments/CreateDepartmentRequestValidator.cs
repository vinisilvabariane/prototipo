using FluentValidation;
using IrisWeb.Application.Communication.Departments.Requests;
using IrisWeb.Domain.Errors;

namespace IrisWeb.Application.Validators.Departments;

public sealed class CreateDepartmentRequestValidator : AbstractValidator<CreateDepartmentRequest>
{
    public CreateDepartmentRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(DepartmentErrors.NameRequired)
            .Must(name => name.Trim().Length <= DepartmentErrors.MaxNameLength)
            .WithMessage(DepartmentErrors.FieldTooLong("O nome do departamento", DepartmentErrors.MaxNameLength));
    }
}
