using FluentValidation;
using MeOne.Application.Communication.Departments.Requests;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.Departments;

public sealed class UpdateDepartmentRequestValidator : AbstractValidator<UpdateDepartmentRequest>
{
    public UpdateDepartmentRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(DepartmentErrors.NameRequired)
            .Must(name => name.Trim().Length <= DepartmentErrors.MaxNameLength)
            .WithMessage(DepartmentErrors.FieldTooLong("O nome do departamento", DepartmentErrors.MaxNameLength));
    }
}
