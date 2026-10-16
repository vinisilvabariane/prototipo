using FluentValidation;
using MeOne.Application.Communication.Workflows.Requests;

namespace MeOne.Application.Validators.Workflows;

public sealed class UpdateWorkflowRequestValidator : AbstractValidator<UpdateWorkflowRequest>
{
    public UpdateWorkflowRequestValidator()
    {
        RuleFor(x => x.Title).ValidWorkflowTitle();
        RuleFor(x => x.Description).ValidWorkflowDescription();
        RuleFor(x => x.Priority).ValidWorkflowPriority();
        RuleFor(x => x.Category).ValidWorkflowCategory();
    }
}
