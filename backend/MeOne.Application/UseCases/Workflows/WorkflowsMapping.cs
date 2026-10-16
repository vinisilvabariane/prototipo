using FluentResults;
using MeOne.Application.Communication.Workflows.Responses;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Application.UseCases.Auth;
using MeOne.Domain.Entities;
using MeOne.Domain.Enums;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.Workflows;

internal static class WorkflowsMapping
{
    public static async Task<Result<ulong>> ActorIdAsync(ICurrentActor currentActor, CancellationToken cancellationToken)
    {
        var actor = await AccessGuard.ActorAsync(currentActor, cancellationToken);

        return actor.IsSuccess ? Result.Ok(actor.Value.UserId) : actor.ToResult<ulong>();
    }

    public static Result Fail(WorkflowRefusal refusal) => Result.Fail(ErrorFor(refusal));

    public static Result<T> Fail<T>(WorkflowRefusal refusal) => Result.Fail<T>(ErrorFor(refusal));

    public static WorkflowSummaryResponse ToSummary(Workflow workflow) => new(
        workflow.Id,
        workflow.Title,
        workflow.Description,
        workflow.Priority,
        workflow.Category,
        workflow.Status,
        workflow.CreatedAt,
        workflow.DueDate,
        workflow.CompletedAt);

    public static async Task<IReadOnlyDictionary<ulong, User>> PeopleAsync(
        IUserRepository users,
        Workflow workflow,
        CancellationToken cancellationToken)
    {
        var ids = workflow.Assignees.Select(assignee => assignee.UserId)
            .Concat(workflow.Steps.Select(step => step.UserId))
            .Concat(workflow.Messages.Select(message => message.UserId))
            .Append(workflow.CreatorId)
            .Distinct()
            .ToList();

        return (await users.GetByIdsIncludingInactiveAsync(ids, cancellationToken)).ToDictionary(user => user.Id);
    }

    public static string NameOf(IReadOnlyDictionary<ulong, User> people, ulong userId) =>
        people.TryGetValue(userId, out var user) ? user.Name : WorkflowOutcomeTexts.UnknownUser;

    public static string Describe(User user) => $"{user.Name} ({user.Registration})";

    public static WorkflowDetailResponse ToDetail(
        Workflow workflow,
        ulong actorId,
        IReadOnlyDictionary<ulong, User> people)
    {
        var assignees = workflow.Assignees
            .OrderByDescending(assignee => assignee.AssignedAt)
            .ThenByDescending(assignee => assignee.Id)
            .Select(assignee => new WorkflowAssigneeResponse(
                assignee.UserId,
                NameOf(people, assignee.UserId),
                people.TryGetValue(assignee.UserId, out var person) ? person.Registration : 0,
                assignee.AssignedAt,
                workflow.HasApproved(assignee.UserId)))
            .ToList();

        var attachments = workflow.Attachments
            .OrderByDescending(attachment => attachment.UploadedAt)
            .ThenByDescending(attachment => attachment.Id)
            .Select(attachment => new WorkflowAttachmentResponse(
                attachment.Id,
                attachment.OriginalName,
                attachment.ContentType,
                attachment.Size,
                attachment.UploadedAt))
            .ToList();

        var steps = workflow.Steps
            .OrderByDescending(step => step.CreatedAt)
            .ThenByDescending(step => step.Id)
            .Select(step => new WorkflowStepResponse(
                step.Id,
                step.Action,
                step.Description,
                step.Justification,
                step.UserId,
                NameOf(people, step.UserId),
                step.CreatedAt,
                step.Review,
                workflow.ReviewRoleOf(step, actorId)))
            .ToList();

        var permissions = new WorkflowPermissionsResponse(
            workflow.IsCreator(actorId),
            workflow.IsAssignee(actorId),
            workflow.CheckApprove(actorId) is null,
            workflow.CheckReject(actorId) is null,
            workflow.CheckEdit(actorId) is null,
            workflow.IsCreator(actorId) && workflow.IsOpen);

        return new WorkflowDetailResponse(
            workflow.Id,
            workflow.Title,
            workflow.Description,
            workflow.Priority,
            workflow.Category,
            workflow.Status,
            workflow.CreatedAt,
            workflow.DueDate,
            workflow.CompletedAt,
            workflow.CreatorId,
            NameOf(people, workflow.CreatorId),
            assignees,
            attachments,
            steps,
            permissions);
    }

    private static IError ErrorFor(WorkflowRefusal refusal) => refusal.Kind switch
    {
        WorkflowRefusalKind.Forbidden => new ForbiddenError(refusal.Message),
        WorkflowRefusalKind.NotFound => new NotFoundError(refusal.Message),
        _ => new Error(refusal.Message),
    };
}
