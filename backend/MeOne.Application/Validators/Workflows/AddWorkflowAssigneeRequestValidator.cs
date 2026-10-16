using FluentValidation;
using MeOne.Application.Communication.Workflows.Requests;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.Workflows;

public sealed class AddWorkflowAssigneeRequestValidator : AbstractValidator<AddWorkflowAssigneeRequest>
{
    public AddWorkflowAssigneeRequestValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0UL).WithMessage(WorkflowErrors.UserRequired);
    }
}
