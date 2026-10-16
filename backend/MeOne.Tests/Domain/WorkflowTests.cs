using MeOne.Domain.Entities;
using MeOne.Domain.Enums;
using MeOne.Domain.Errors;

namespace MeOne.Tests.Domain;

public sealed class WorkflowTests
{
    private const ulong CreatorId = 1;
    private const ulong AliceId = 2;
    private const ulong BobId = 3;
    private const ulong OutsiderId = 9;

    private static readonly DateTime Now = new(2026, 10, 15, 12, 0, 0, DateTimeKind.Utc);

    private static Workflow Open(params ulong[] assignees)
    {
        var workflow = Workflow.Create(
            "  Review the control plan ",
            " Description ",
            WorkflowPriority.High,
            WorkflowCategory.Standards,
            new DateOnly(2026, 11, 1),
            CreatorId,
            assignees.Length == 0 ? [AliceId, BobId] : assignees,
            Now);

        NumberSteps(workflow);
        return workflow;
    }

    private static void NumberSteps(Workflow workflow)
    {
        ulong next = workflow.Steps.Max(step => step.Id);

        foreach (var step in workflow.Steps.Where(step => step.Id == 0))
            step.Id = ++next;
    }

    private static WorkflowStep Rejection(Workflow workflow, ulong userId)
    {
        workflow.Reject(userId, "Not acceptable", Now);
        NumberSteps(workflow);

        return workflow.Steps.Single(step => step.Action == WorkflowAction.Rejection && step.UserId == userId);
    }

    [Fact]
    public void Create_NormalizesTextAndLinksTheCreatorAsAssignee()
    {
        var workflow = Open();

        Assert.Equal("Review the control plan", workflow.Title);
        Assert.Equal("Description", workflow.Description);
        Assert.Equal(WorkflowStatus.InProgress, workflow.Status);
        Assert.Equal(new[] { CreatorId, AliceId, BobId }, workflow.Assignees.Select(assignee => assignee.UserId).Order());
    }

    [Fact]
    public void Create_RecordsCreationAndTheCreatorsAutomaticApproval()
    {
        var workflow = Open();

        Assert.Equal(
            new[] { WorkflowAction.Created, WorkflowAction.Approval },
            workflow.Steps.Select(step => step.Action));
        Assert.True(workflow.HasApproved(CreatorId));
        Assert.False(workflow.HasApproved(AliceId));
    }

    [Fact]
    public void Create_DoesNotDuplicateTheCreatorWhenListed()
    {
        var workflow = Open(CreatorId, AliceId, AliceId);

        Assert.Equal(2, workflow.Assignees.Count);
    }

    [Fact]
    public void Create_WithOnlyTheCreator_IsApprovedRightAway()
    {
        var workflow = Open(CreatorId);

        Assert.Equal(WorkflowStatus.Approved, workflow.Status);
        Assert.Equal(Now, workflow.CompletedAt);
        Assert.Equal(WorkflowAction.FinalApproval, workflow.Steps.Last().Action);
    }

    [Fact]
    public void Approve_ByEveryAssignee_ClosesTheWorkflow()
    {
        var workflow = Open();

        Assert.False(workflow.Approve(AliceId, "Looks good", Now));
        Assert.Equal(WorkflowStatus.InProgress, workflow.Status);

        Assert.True(workflow.Approve(BobId, "Fine by me", Now));
        Assert.Equal(WorkflowStatus.Approved, workflow.Status);
        Assert.Equal(Now, workflow.CompletedAt);
        Assert.Equal(WorkflowAction.FinalApproval, workflow.Steps.Last().Action);
    }

    [Fact]
    public void Approve_Twice_IsRefused()
    {
        var workflow = Open();
        workflow.Approve(AliceId, "Looks good", Now);

        var refusal = workflow.CheckApprove(AliceId);

        Assert.Equal(WorkflowErrors.AlreadyApproved, refusal?.Message);
        Assert.Equal(WorkflowRefusalKind.Invalid, refusal?.Kind);
    }

    [Fact]
    public void Approve_ByTheCreator_IsRefusedBecauseTheyAlreadyApproved()
    {
        var workflow = Open();

        Assert.Equal(WorkflowErrors.AlreadyApproved, workflow.CheckApprove(CreatorId)?.Message);
    }

    [Fact]
    public void Approve_ByAnOutsider_IsForbidden()
    {
        var workflow = Open();

        var refusal = workflow.CheckApprove(OutsiderId);

        Assert.Equal(WorkflowRefusalKind.Forbidden, refusal?.Kind);
        Assert.Equal(WorkflowErrors.NotAssignee, refusal?.Message);
    }

    [Fact]
    public void Approve_ByAnOutsider_ThrowsDomainRuleException()
    {
        var workflow = Open();

        Assert.Throws<DomainRuleException>(() => workflow.Approve(OutsiderId, "Sneaky", Now));
    }

    [Fact]
    public void Reject_OpensAReviewWithTheCreator()
    {
        var workflow = Open();

        var step = Rejection(workflow, AliceId);

        Assert.Equal(ReviewStage.CreatorReview, step.Review);
        Assert.Equal("Not acceptable", step.Justification);
        Assert.Equal(WorkflowStatus.InProgress, workflow.Status);
        Assert.True(workflow.HasRejectionAwaitingCreator);
    }

    [Fact]
    public void Reject_BlocksApprovingAndRejectingAgainWhileOpen()
    {
        var workflow = Open();
        Rejection(workflow, AliceId);

        Assert.Equal(WorkflowErrors.RejectionUnderCreatorReview, workflow.CheckApprove(AliceId)?.Message);
        Assert.Equal(WorkflowErrors.RejectionAlreadyOpen, workflow.CheckReject(AliceId)?.Message);
    }

    [Fact]
    public void Reject_AfterApproving_IsRefused()
    {
        var workflow = Open();
        workflow.Approve(AliceId, "Looks good", Now);

        Assert.Equal(WorkflowErrors.AlreadyApproved, workflow.CheckReject(AliceId)?.Message);
    }

    [Fact]
    public void ReturnToAssignee_HandsTheReviewBackAndKeepsTheOriginalJustification()
    {
        var workflow = Open();
        var step = Rejection(workflow, AliceId);

        workflow.ReturnToAssignee(CreatorId, step.Id, "Please reconsider", Now);

        Assert.Equal(ReviewStage.AssigneeReview, step.Review);
        Assert.Equal("Not acceptable", step.Justification);
        var message = Assert.Single(workflow.Messages);
        Assert.Equal("Please reconsider", message.Text);
        Assert.Equal(CreatorId, message.UserId);
        Assert.Equal(step.Id, message.StepId);
    }

    [Fact]
    public void ReturnToAssignee_ByTheAssignee_IsForbidden()
    {
        var workflow = Open();
        var step = Rejection(workflow, AliceId);

        Assert.Equal(WorkflowRefusalKind.Forbidden, workflow.CheckReturnToAssignee(AliceId, step.Id)?.Kind);
    }

    [Fact]
    public void ReturnToAssignee_WhenTheReviewIsNotWithTheCreator_IsRefused()
    {
        var workflow = Open();
        var step = Rejection(workflow, AliceId);
        workflow.ReturnToAssignee(CreatorId, step.Id, "Please reconsider", Now);

        Assert.Equal(
            WorkflowErrors.RejectionNotAwaitingCreator,
            workflow.CheckReturnToAssignee(CreatorId, step.Id)?.Message);
    }

    [Fact]
    public void ReturnToAssignee_WithUnknownStep_IsNotFound()
    {
        var workflow = Open();

        Assert.Equal(WorkflowRefusalKind.NotFound, workflow.CheckReturnToAssignee(CreatorId, 999)?.Kind);
    }

    [Fact]
    public void ReturnToCreator_SendsTheReviewBackToTheCreator()
    {
        var workflow = Open();
        var step = Rejection(workflow, AliceId);
        workflow.ReturnToAssignee(CreatorId, step.Id, "Please reconsider", Now);

        workflow.ReturnToCreator(AliceId, step.Id, "I still disagree", Now);

        Assert.Equal(ReviewStage.CreatorReview, step.Review);
        Assert.Equal(2, workflow.Messages.Count);
    }

    [Fact]
    public void ReturnToCreator_ByAnotherAssignee_IsForbidden()
    {
        var workflow = Open();
        var step = Rejection(workflow, AliceId);
        workflow.ReturnToAssignee(CreatorId, step.Id, "Please reconsider", Now);

        Assert.Equal(WorkflowRefusalKind.Forbidden, workflow.CheckReturnToCreator(BobId, step.Id)?.Kind);
    }

    [Fact]
    public void ReturnToCreator_WhenTheReviewIsWithTheCreator_IsRefused()
    {
        var workflow = Open();
        var step = Rejection(workflow, AliceId);

        Assert.Equal(
            WorkflowErrors.RejectionNotAwaitingAssignee,
            workflow.CheckReturnToCreator(AliceId, step.Id)?.Message);
    }

    [Fact]
    public void ReturnToCreator_OnAStepThatIsNotARejection_IsRefused()
    {
        var workflow = Open();
        var created = workflow.Steps.First(step => step.Action == WorkflowAction.Created);

        Assert.Equal(WorkflowErrors.StepIsNotRejection, workflow.CheckReturnToCreator(CreatorId, created.Id)?.Message);
    }

    [Fact]
    public void Approve_AfterTheCreatorReturnedTheReview_ResolvesTheRejection()
    {
        var workflow = Open();
        var step = Rejection(workflow, AliceId);
        workflow.ReturnToAssignee(CreatorId, step.Id, "Please reconsider", Now);

        workflow.Approve(AliceId, "You convinced me", Now);

        Assert.Equal(ReviewStage.None, step.Review);
        Assert.True(workflow.HasApproved(AliceId));
        Assert.Null(workflow.PendingRejectionOf(AliceId));
    }

    [Fact]
    public void AcceptRejection_RejectsTheWorkflowAndClearsEveryReview()
    {
        var workflow = Open();
        var alice = Rejection(workflow, AliceId);

        workflow.AcceptRejection(CreatorId, "Agreed, dropping it", Now);

        Assert.Equal(WorkflowStatus.Rejected, workflow.Status);
        Assert.Equal(Now, workflow.CompletedAt);
        Assert.Equal(ReviewStage.None, alice.Review);
        Assert.Equal(WorkflowAction.FinalRejection, workflow.Steps.Last().Action);
        Assert.Equal("Agreed, dropping it", workflow.Steps.Last().Justification);
    }

    [Fact]
    public void AcceptRejection_ByAnAssignee_IsForbidden()
    {
        var workflow = Open();
        Rejection(workflow, AliceId);

        Assert.Equal(WorkflowRefusalKind.Forbidden, workflow.CheckAcceptRejection(BobId)?.Kind);
    }

    [Fact]
    public void AcceptRejection_WithoutAPendingRejection_IsRefused()
    {
        var workflow = Open();

        Assert.Equal(WorkflowErrors.NoRejectionAwaitingCreator, workflow.CheckAcceptRejection(CreatorId)?.Message);
    }

    [Fact]
    public void AcceptRejection_WhileTheReviewIsWithTheAssignee_IsRefused()
    {
        var workflow = Open();
        var step = Rejection(workflow, AliceId);
        workflow.ReturnToAssignee(CreatorId, step.Id, "Please reconsider", Now);

        Assert.Equal(WorkflowErrors.NoRejectionAwaitingCreator, workflow.CheckAcceptRejection(CreatorId)?.Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void FinishedWorkflow_RefusesEveryAction(bool approved)
    {
        var workflow = Open(CreatorId, AliceId);

        if (approved)
        {
            workflow.Approve(AliceId, "Looks good", Now);
        }
        else
        {
            Rejection(workflow, AliceId);
            workflow.AcceptRejection(CreatorId, "Dropping it", Now);
        }

        var expected = WorkflowErrors.AlreadyFinished(approved ? "aprovado" : "rejeitado");

        Assert.Equal(expected, workflow.CheckEdit(CreatorId)?.Message);
        Assert.Equal(expected, workflow.CheckApprove(AliceId)?.Message);
        Assert.Equal(expected, workflow.CheckReject(AliceId)?.Message);
        Assert.Equal(expected, workflow.CheckAddAssignee(CreatorId, OutsiderId)?.Message);
        Assert.Equal(expected, workflow.CheckReplaceAssignee(CreatorId, AliceId, OutsiderId)?.Message);
    }

    [Fact]
    public void Edit_ChangesTheFieldsAndRecordsTheEdit()
    {
        var workflow = Open();
        var version = workflow.Version;

        var changed = workflow.Edit(CreatorId, " New title ", "New description", WorkflowPriority.Low, null, Now);

        Assert.True(changed);
        Assert.Equal("New title", workflow.Title);
        Assert.Equal(WorkflowPriority.Low, workflow.Priority);
        Assert.Null(workflow.Category);
        Assert.Equal(WorkflowAction.Edited, workflow.Steps.Last().Action);
        Assert.True(workflow.Version > version);
    }

    [Fact]
    public void Edit_WithoutChanges_RecordsNothing()
    {
        var workflow = Open();
        var version = workflow.Version;
        var steps = workflow.Steps.Count;

        var changed = workflow.Edit(
            CreatorId,
            workflow.Title,
            workflow.Description,
            workflow.Priority,
            workflow.Category,
            Now);

        Assert.False(changed);
        Assert.Equal(steps, workflow.Steps.Count);
        Assert.Equal(version, workflow.Version);
    }

    [Fact]
    public void Edit_ByAnAssignee_IsForbidden()
    {
        var workflow = Open();

        Assert.Equal(WorkflowErrors.OnlyCreatorEdits, workflow.CheckEdit(AliceId)?.Message);
    }

    [Fact]
    public void AddAssignee_LinksTheUserAsAPendingApprover()
    {
        var workflow = Open(CreatorId, AliceId);

        workflow.AddAssignee(CreatorId, BobId, "Responsável adicionado: Bob", Now);

        Assert.True(workflow.IsAssignee(BobId));
        Assert.Equal(WorkflowAction.AssigneeAdded, workflow.Steps.Last().Action);
        Assert.False(workflow.EveryoneApproved);
    }

    [Fact]
    public void AddAssignee_RefusesDuplicatesAndTheCreator()
    {
        var workflow = Open();

        Assert.Equal(WorkflowErrors.AlreadyAssignee, workflow.CheckAddAssignee(CreatorId, AliceId)?.Message);
        Assert.Equal(WorkflowErrors.CreatorIsAlreadyAssignee, workflow.CheckAddAssignee(CreatorId, CreatorId)?.Message);
    }

    [Fact]
    public void AddAssignee_ByAnAssignee_IsForbidden()
    {
        var workflow = Open();

        Assert.Equal(WorkflowErrors.OnlyCreatorManagesAssignees, workflow.CheckAddAssignee(AliceId, OutsiderId)?.Message);
    }

    [Fact]
    public void ReplaceAssignee_SwapsTheUsersAndRecordsIt()
    {
        var workflow = Open();

        workflow.ReplaceAssignee(CreatorId, AliceId, OutsiderId, "Substituição", Now);

        Assert.False(workflow.IsAssignee(AliceId));
        Assert.True(workflow.IsAssignee(OutsiderId));
        Assert.Equal(WorkflowAction.AssigneeReplaced, workflow.Steps.Last().Action);
        Assert.Equal(WorkflowStatus.InProgress, workflow.Status);
    }

    [Fact]
    public void ReplaceAssignee_ClearsTheReviewsOfTheRemovedUser()
    {
        var workflow = Open();
        var step = Rejection(workflow, AliceId);

        workflow.ReplaceAssignee(CreatorId, AliceId, OutsiderId, "Substituição", Now);

        Assert.Equal(ReviewStage.None, step.Review);
        Assert.False(workflow.HasRejectionAwaitingCreator);
    }

    [Fact]
    public void ReplaceAssignee_RefusesTheCreatorAnApprovedUserAndListedUsers()
    {
        var workflow = Open();
        workflow.Approve(AliceId, "Looks good", Now);

        Assert.Equal(WorkflowErrors.CannotRemoveCreator, workflow.CheckReplaceAssignee(CreatorId, CreatorId, OutsiderId)?.Message);
        Assert.Equal(WorkflowErrors.AssigneeAlreadyApproved, workflow.CheckReplaceAssignee(CreatorId, AliceId, OutsiderId)?.Message);
        Assert.Equal(WorkflowErrors.NewAssigneeAlreadyListed, workflow.CheckReplaceAssignee(CreatorId, BobId, AliceId)?.Message);
        Assert.Equal(WorkflowRefusalKind.NotFound, workflow.CheckReplaceAssignee(CreatorId, OutsiderId, 77)?.Kind);
    }

    [Fact]
    public void ReviewRoleOf_PointsToWhoseTurnItIs()
    {
        var workflow = Open();
        var step = Rejection(workflow, AliceId);

        Assert.Equal(ReviewRole.Creator, workflow.ReviewRoleOf(step, CreatorId));
        Assert.Equal(ReviewRole.None, workflow.ReviewRoleOf(step, AliceId));

        workflow.ReturnToAssignee(CreatorId, step.Id, "Please reconsider", Now);

        Assert.Equal(ReviewRole.Assignee, workflow.ReviewRoleOf(step, AliceId));
        Assert.Equal(ReviewRole.None, workflow.ReviewRoleOf(step, CreatorId));
        Assert.Equal(ReviewRole.None, workflow.ReviewRoleOf(step, BobId));
    }

    [Fact]
    public void EveryMutation_BumpsTheVersionForConcurrencyControl()
    {
        var workflow = Open();
        var version = workflow.Version;

        workflow.Approve(AliceId, "Looks good", Now);
        Assert.True(workflow.Version > version);

        version = workflow.Version;
        Rejection(workflow, BobId);
        Assert.True(workflow.Version > version);
    }
}
