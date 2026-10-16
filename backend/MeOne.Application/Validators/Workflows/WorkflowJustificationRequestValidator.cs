using FluentValidation;
using MeOne.Application.Communication.Workflows.Requests;

namespace MeOne.Application.Validators.Workflows;

public sealed class WorkflowJustificationRequestValidator : AbstractValidator<WorkflowJustificationRequest>
{
    public WorkflowJustificationRequestValidator()
    {
        RuleFor(x => x.Justification).ValidJustification();
    }
}
