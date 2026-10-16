using FluentValidation;
using MeOne.Application.Communication.Workflows.Requests;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.Workflows;

public sealed class CreateWorkflowRequestValidator : AbstractValidator<CreateWorkflowRequest>
{
    public CreateWorkflowRequestValidator()
    {
        RuleFor(x => x.Title).ValidWorkflowTitle();
        RuleFor(x => x.Description).ValidWorkflowDescription();
        RuleFor(x => x.Priority).ValidWorkflowPriority();
        RuleFor(x => x.Category).ValidWorkflowCategory();
        RuleFor(x => x.DueDate).NotNull().WithMessage(WorkflowErrors.DueDateRequired);
    }
}
