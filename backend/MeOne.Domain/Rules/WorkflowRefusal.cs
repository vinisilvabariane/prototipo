using MeOne.Domain.Enums;

namespace MeOne.Domain.Rules;

public readonly record struct WorkflowRefusal(WorkflowRefusalKind Kind, string Message)
{
    public static WorkflowRefusal Invalid(string message) => new(WorkflowRefusalKind.Invalid, message);

    public static WorkflowRefusal Forbidden(string message) => new(WorkflowRefusalKind.Forbidden, message);

    public static WorkflowRefusal NotFound(string message) => new(WorkflowRefusalKind.NotFound, message);
}
