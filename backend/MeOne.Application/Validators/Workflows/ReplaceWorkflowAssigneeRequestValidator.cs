using FluentValidation;
using MeOne.Application.Communication.Workflows.Requests;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.Workflows;

public sealed class ReplaceWorkflowAssigneeRequestValidator : AbstractValidator<ReplaceWorkflowAssigneeRequest>
{
    public ReplaceWorkflowAssigneeRequestValidator()
    {
        RuleFor(x => x.NewUserId).GreaterThan(0UL).WithMessage(WorkflowErrors.NewAssigneeRequired);
    }
}
