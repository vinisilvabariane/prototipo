using FluentValidation;
using MeOne.Domain.Enums;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.Workflows;

internal static class WorkflowFieldRules
{
    public static IRuleBuilderOptions<T, string> ValidWorkflowTitle<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage(WorkflowErrors.TitleRequired)
            .Must(title => title.Trim().Length <= WorkflowErrors.MaxTitleLength)
            .WithMessage(WorkflowErrors.FieldTooLong("O título", WorkflowErrors.MaxTitleLength));

    public static IRuleBuilderOptions<T, string?> ValidWorkflowDescription<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(description => (description ?? string.Empty).Trim().Length <= WorkflowErrors.MaxDescriptionLength)
            .WithMessage(WorkflowErrors.FieldTooLong("A descrição", WorkflowErrors.MaxDescriptionLength));

    public static IRuleBuilderOptions<T, WorkflowPriority> ValidWorkflowPriority<T>(
        this IRuleBuilder<T, WorkflowPriority> rule) =>
        rule.IsInEnum().WithMessage(WorkflowErrors.PriorityInvalid);

    public static IRuleBuilderOptions<T, WorkflowCategory?> ValidWorkflowCategory<T>(
        this IRuleBuilder<T, WorkflowCategory?> rule) =>
        rule.Must(category => category is null || Enum.IsDefined(category.Value))
            .WithMessage(WorkflowErrors.CategoryInvalid);

    public static IRuleBuilderOptions<T, string> ValidJustification<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage(WorkflowErrors.JustificationRequired)
            .Must(text => text.Trim().Length <= WorkflowErrors.MaxJustificationLength)
            .WithMessage(WorkflowErrors.FieldTooLong("A justificativa", WorkflowErrors.MaxJustificationLength));
}
