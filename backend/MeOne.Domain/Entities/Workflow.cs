using MeOne.Domain.Enums;
using MeOne.Domain.Errors;
using MeOne.Domain.Rules;

namespace MeOne.Domain.Entities;

public sealed class Workflow : Entity<ulong>
{
    private readonly List<WorkflowAssignee> _assignees = [];

    private readonly List<WorkflowStep> _steps = [];

    private readonly List<WorkflowAttachment> _attachments = [];

    private readonly List<WorkflowMessage> _messages = [];

    private Workflow() { }

    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public WorkflowPriority Priority { get; private set; }

    public WorkflowCategory? Category { get; private set; }

    public WorkflowStatus Status { get; private set; } = WorkflowStatus.InProgress;

    public ulong CreatorId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? CompletedAt { get; private set; }

    public DateOnly? DueDate { get; private set; }

    public uint Version { get; private set; }

    public IReadOnlyCollection<WorkflowAssignee> Assignees => _assignees;

    public IReadOnlyCollection<WorkflowStep> Steps => _steps;

    public IReadOnlyCollection<WorkflowAttachment> Attachments => _attachments;

    public IReadOnlyCollection<WorkflowMessage> Messages => _messages;

    public bool IsOpen => Status == WorkflowStatus.InProgress;

    public static Workflow Create(
        string title,
        string description,
        WorkflowPriority priority,
        WorkflowCategory? category,
        DateOnly? dueDate,
        ulong creatorId,
        IEnumerable<ulong> assigneeIds,
        DateTime now)
    {
        var workflow = new Workflow
        {
            Title = title.Trim(),
            Description = description.Trim(),
            Priority = priority,
            Category = category,
            DueDate = dueDate,
            CreatorId = creatorId,
            CreatedAt = now,
        };

        foreach (var userId in assigneeIds.Append(creatorId).Distinct())
            workflow._assignees.Add(WorkflowAssignee.Link(userId, now));

        workflow.AddStep(creatorId, WorkflowAction.Created, WorkflowHistoryTexts.Created, WorkflowHistoryTexts.CreatedNote, now);
        workflow.AddStep(
            creatorId,
            WorkflowAction.Approval,
            WorkflowHistoryTexts.CreatorAutoApproval,
            WorkflowHistoryTexts.CreatorAutoApprovalNote,
            now);

        workflow.CompleteIfEveryoneApproved(
            creatorId,
            now,
            WorkflowHistoryTexts.FinalApproval,
            WorkflowHistoryTexts.FinalApprovalNote);

        return workflow;
    }

    public bool IsCreator(ulong userId) => CreatorId == userId;

    public bool IsAssignee(ulong userId) => _assignees.Any(assignee => assignee.UserId == userId);

    public bool HasApproved(ulong userId) =>
        _steps.Any(step => step.Action == WorkflowAction.Approval && step.UserId == userId);

    public bool EveryoneApproved => _assignees.All(assignee => HasApproved(assignee.UserId));

    public WorkflowStep? FindStep(ulong stepId) => _steps.FirstOrDefault(step => step.Id == stepId);

    public WorkflowStep? PendingRejectionOf(ulong userId) => _steps
        .Where(step => step.Action == WorkflowAction.Rejection && step.UserId == userId && step.Review != ReviewStage.None)
        .OrderByDescending(step => step.Id)
        .FirstOrDefault();

    public bool HasRejectionAwaitingCreator =>
        _steps.Any(step => step.Action == WorkflowAction.Rejection && step.Review == ReviewStage.CreatorReview);

    public ReviewRole ReviewRoleOf(WorkflowStep step, ulong userId)
    {
        if (!IsOpen || step.Action != WorkflowAction.Rejection)
            return ReviewRole.None;

        if (step.Review == ReviewStage.CreatorReview && IsCreator(userId))
            return ReviewRole.Creator;

        return step.Review == ReviewStage.AssigneeReview && step.UserId == userId
            ? ReviewRole.Assignee
            : ReviewRole.None;
    }

    public WorkflowRefusal? CheckEdit(ulong actorId) =>
        OnlyCreator(actorId, WorkflowErrors.OnlyCreatorEdits) ?? EnsureOpen();

    public WorkflowRefusal? CheckApprove(ulong actorId)
    {
        if (!IsAssignee(actorId))
            return WorkflowRefusal.Forbidden(WorkflowErrors.NotAssignee);

        if (EnsureOpen() is { } finished)
            return finished;

        if (HasApproved(actorId))
            return WorkflowRefusal.Invalid(WorkflowErrors.AlreadyApproved);

        return PendingRejectionOf(actorId)?.Review == ReviewStage.CreatorReview
            ? WorkflowRefusal.Invalid(WorkflowErrors.RejectionUnderCreatorReview)
            : null;
    }

    public WorkflowRefusal? CheckReject(ulong actorId)
    {
        if (!IsAssignee(actorId))
            return WorkflowRefusal.Forbidden(WorkflowErrors.NotAssignee);

        if (EnsureOpen() is { } finished)
            return finished;

        if (HasApproved(actorId))
            return WorkflowRefusal.Invalid(WorkflowErrors.AlreadyApproved);

        return PendingRejectionOf(actorId) is null
            ? null
            : WorkflowRefusal.Invalid(WorkflowErrors.RejectionAlreadyOpen);
    }

    public WorkflowRefusal? CheckAcceptRejection(ulong actorId)
    {
        if (OnlyCreator(actorId, WorkflowErrors.OnlyCreatorReviews) is { } notCreator)
            return notCreator;

        if (EnsureOpen() is { } finished)
            return finished;

        return HasRejectionAwaitingCreator
            ? null
            : WorkflowRefusal.Invalid(WorkflowErrors.NoRejectionAwaitingCreator);
    }

    public WorkflowRefusal? CheckReturnToAssignee(ulong actorId, ulong stepId)
    {
        if (OnlyCreator(actorId, WorkflowErrors.OnlyCreatorReviews) is { } notCreator)
            return notCreator;

        if (EnsureOpen() is { } finished)
            return finished;

        var step = FindStep(stepId);

        if (step is null)
            return WorkflowRefusal.NotFound(WorkflowErrors.StepNotFound);

        return step is { Action: WorkflowAction.Rejection, Review: ReviewStage.CreatorReview }
            ? null
            : WorkflowRefusal.Invalid(WorkflowErrors.RejectionNotAwaitingCreator);
    }

    public WorkflowRefusal? CheckReturnToCreator(ulong actorId, ulong stepId)
    {
        var step = FindStep(stepId);

        if (step is null)
            return WorkflowRefusal.NotFound(WorkflowErrors.StepNotFound);

        if (step.Action != WorkflowAction.Rejection)
            return WorkflowRefusal.Invalid(WorkflowErrors.StepIsNotRejection);

        if (step.UserId != actorId)
            return WorkflowRefusal.Forbidden(WorkflowErrors.OnlyRejecterReviews);

        if (EnsureOpen() is { } finished)
            return finished;

        return step.Review == ReviewStage.AssigneeReview
            ? null
            : WorkflowRefusal.Invalid(WorkflowErrors.RejectionNotAwaitingAssignee);
    }

    public WorkflowRefusal? CheckAddAssignee(ulong actorId, ulong newUserId)
    {
        if (OnlyCreator(actorId, WorkflowErrors.OnlyCreatorManagesAssignees) is { } notCreator)
            return notCreator;

        if (EnsureOpen() is { } finished)
            return finished;

        if (IsCreator(newUserId))
            return WorkflowRefusal.Invalid(WorkflowErrors.CreatorIsAlreadyAssignee);

        return IsAssignee(newUserId)
            ? WorkflowRefusal.Invalid(WorkflowErrors.AlreadyAssignee)
            : null;
    }

    public WorkflowRefusal? CheckReplaceAssignee(ulong actorId, ulong oldUserId, ulong newUserId)
    {
        if (OnlyCreator(actorId, WorkflowErrors.OnlyCreatorManagesAssignees) is { } notCreator)
            return notCreator;

        if (EnsureOpen() is { } finished)
            return finished;

        if (!IsAssignee(oldUserId))
            return WorkflowRefusal.NotFound(WorkflowErrors.AssigneeNotFound);

        if (IsCreator(oldUserId))
            return WorkflowRefusal.Invalid(WorkflowErrors.CannotRemoveCreator);

        if (HasApproved(oldUserId))
            return WorkflowRefusal.Invalid(WorkflowErrors.AssigneeAlreadyApproved);

        return IsAssignee(newUserId)
            ? WorkflowRefusal.Invalid(WorkflowErrors.NewAssigneeAlreadyListed)
            : null;
    }

    public bool Edit(
        ulong actorId,
        string title,
        string description,
        WorkflowPriority priority,
        WorkflowCategory? category,
        DateTime now)
    {
        Ensure(CheckEdit(actorId));

        var normalizedTitle = title.Trim();
        var normalizedDescription = description.Trim();

        if (Title == normalizedTitle &&
            Description == normalizedDescription &&
            Priority == priority &&
            Category == category)
        {
            return false;
        }

        Title = normalizedTitle;
        Description = normalizedDescription;
        Priority = priority;
        Category = category;

        AddStep(actorId, WorkflowAction.Edited, WorkflowHistoryTexts.Edited, WorkflowHistoryTexts.EditedNote, now);
        Touch();
        return true;
    }

    public bool Approve(ulong actorId, string justification, DateTime now)
    {
        Ensure(CheckApprove(actorId));

        PendingRejectionOf(actorId)?.SetReview(ReviewStage.None);

        AddStep(actorId, WorkflowAction.Approval, WorkflowHistoryTexts.Approved, justification, now);
        Touch();

        return CompleteIfEveryoneApproved(
            actorId,
            now,
            WorkflowHistoryTexts.FinalApproval,
            WorkflowHistoryTexts.FinalApprovalNote);
    }

    public void Reject(ulong actorId, string justification, DateTime now)
    {
        Ensure(CheckReject(actorId));

        AddStep(
            actorId,
            WorkflowAction.Rejection,
            WorkflowHistoryTexts.Rejected,
            justification,
            now,
            ReviewStage.CreatorReview);

        Touch();
    }

    public void AcceptRejection(ulong actorId, string justification, DateTime now)
    {
        Ensure(CheckAcceptRejection(actorId));

        foreach (var step in _steps.Where(step => step.Review != ReviewStage.None))
            step.SetReview(ReviewStage.None);

        Status = WorkflowStatus.Rejected;
        CompletedAt = now;

        AddStep(actorId, WorkflowAction.FinalRejection, WorkflowHistoryTexts.FinalRejection, justification, now);
        Touch();
    }

    public void ReturnToAssignee(ulong actorId, ulong stepId, string text, DateTime now)
    {
        Ensure(CheckReturnToAssignee(actorId, stepId));

        FindStep(stepId)!.SetReview(ReviewStage.AssigneeReview);
        _messages.Add(WorkflowMessage.Write(stepId, actorId, text, now));
        Touch();
    }

    public void ReturnToCreator(ulong actorId, ulong stepId, string text, DateTime now)
    {
        Ensure(CheckReturnToCreator(actorId, stepId));

        FindStep(stepId)!.SetReview(ReviewStage.CreatorReview);
        _messages.Add(WorkflowMessage.Write(stepId, actorId, text, now));
        Touch();
    }

    public void AddAssignee(ulong actorId, ulong userId, string description, DateTime now)
    {
        Ensure(CheckAddAssignee(actorId, userId));

        _assignees.Add(WorkflowAssignee.Link(userId, now));
        AddStep(actorId, WorkflowAction.AssigneeAdded, description, null, now);
        Touch();
    }

    public bool ReplaceAssignee(ulong actorId, ulong oldUserId, ulong newUserId, string description, DateTime now)
    {
        Ensure(CheckReplaceAssignee(actorId, oldUserId, newUserId));

        _assignees.RemoveAll(assignee => assignee.UserId == oldUserId);
        _assignees.Add(WorkflowAssignee.Link(newUserId, now));

        foreach (var step in _steps.Where(step => step.UserId == oldUserId && step.Review != ReviewStage.None))
            step.SetReview(ReviewStage.None);

        AddStep(actorId, WorkflowAction.AssigneeReplaced, description, WorkflowHistoryTexts.AssigneeReplacedNote, now);
        Touch();

        return CompleteIfEveryoneApproved(
            actorId,
            now,
            WorkflowHistoryTexts.FinalApprovalAfterReplacement,
            WorkflowHistoryTexts.FinalApprovalAfterReplacementNote);
    }

    public void Attach(WorkflowAttachment attachment) => _attachments.Add(attachment);

    private bool CompleteIfEveryoneApproved(ulong actorId, DateTime now, string description, string justification)
    {
        if (!IsOpen || !EveryoneApproved)
            return false;

        Status = WorkflowStatus.Approved;
        CompletedAt = now;

        AddStep(actorId, WorkflowAction.FinalApproval, description, justification, now);
        Touch();
        return true;
    }

    private void AddStep(
        ulong userId,
        WorkflowAction action,
        string description,
        string? justification,
        DateTime now,
        ReviewStage review = ReviewStage.None) =>
        _steps.Add(WorkflowStep.Record(userId, action, description, justification, now, review));

    private WorkflowRefusal? OnlyCreator(ulong actorId, string message) =>
        IsCreator(actorId) ? null : WorkflowRefusal.Forbidden(message);

    private WorkflowRefusal? EnsureOpen() => IsOpen
        ? null
        : WorkflowRefusal.Invalid(WorkflowErrors.AlreadyFinished(Status == WorkflowStatus.Approved ? "aprovado" : "rejeitado"));

    private static void Ensure(WorkflowRefusal? refusal)
    {
        if (refusal is { } violation)
            throw new DomainRuleException(violation.Message);
    }

    private void Touch() => Version++;
}
