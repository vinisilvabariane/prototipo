using MeOne.Application.Communication.Workflows.Dtos;
using MeOne.Application.Communication.Workflows.Requests;
using MeOne.Application.Errors;
using MeOne.Application.UseCases.Workflows;
using MeOne.Domain.Enums;
using MeOne.Domain.Errors;
using MeOne.Domain.Rules;
using MeOne.Tests.Fakes;

namespace MeOne.Tests.Application;

public sealed class WorkflowUseCaseTests
{
    private static CreateWorkflowRequest NewRequest(
        string title = "Review the control plan",
        DateOnly? dueDate = null,
        params ulong[] assignees) =>
        new(
            title,
            "Description",
            WorkflowPriority.High,
            WorkflowCategory.Requirements,
            dueDate ?? DateOnly.FromDateTime(DateTime.UtcNow).AddDays(5),
            assignees);

    private static CreateWorkflowUseCase Creator(WorkflowFixture f, FakeCurrentActor actor) =>
        new(f.Workflows, f.Users, f.Storage, actor);

    private static WorkflowJustificationRequest Justify(string text = "Because it is right") => new(text);

    [Fact]
    public async Task Create_StoresTheWorkflowWithTheCreatorAsAssignee()
    {
        var f = new WorkflowFixture();

        var result = await Creator(f, WorkflowFixture.Actor(f.Creator)).ExecuteAsync(
            NewRequest(assignees: [f.Alice.Id, f.Bob.Id]),
            [],
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var stored = Assert.Single(f.Workflows.Stored);
        Assert.Equal(f.Creator.Id, stored.CreatorId);
        Assert.Equal(3, stored.Assignees.Count);
        Assert.Equal(WorkflowStatus.InProgress, result.Value.Status);
        Assert.Equal(stored.Id, result.Value.Id);
    }

    [Fact]
    public async Task Create_WithOnlyTheCreator_IsApprovedImmediately()
    {
        var f = new WorkflowFixture();

        var result = await Creator(f, WorkflowFixture.Actor(f.Creator)).ExecuteAsync(
            NewRequest(),
            [],
            CancellationToken.None);

        Assert.Equal(WorkflowStatus.Approved, result.Value.Status);
    }

    [Fact]
    public async Task Create_WithDueDateTooFarInThePast_Fails()
    {
        var f = new WorkflowFixture();
        var past = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-2);

        var result = await Creator(f, WorkflowFixture.Actor(f.Creator)).ExecuteAsync(
            NewRequest(dueDate: past),
            [],
            CancellationToken.None);

        Assert.Equal(WorkflowErrors.DueDateInPast, Assert.Single(result.Errors).Message);
        Assert.Empty(f.Workflows.Stored);
    }

    [Fact]
    public async Task Create_WithYesterdaysUtcDueDate_IsAcceptedForTimezoneTolerance()
    {
        var f = new WorkflowFixture();
        var yesterday = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);

        var result = await Creator(f, WorkflowFixture.Actor(f.Creator)).ExecuteAsync(
            NewRequest(dueDate: yesterday),
            [],
            CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Create_WithUnknownAssignee_Fails()
    {
        var f = new WorkflowFixture();

        var result = await Creator(f, WorkflowFixture.Actor(f.Creator)).ExecuteAsync(
            NewRequest(assignees: [f.Alice.Id, 999]),
            [],
            CancellationToken.None);

        Assert.Equal(WorkflowErrors.UnknownAssignees, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Create_WithDeactivatedAssignee_Fails()
    {
        var f = new WorkflowFixture();
        f.Alice.Deactivate(modifiedBy: 1);

        var result = await Creator(f, WorkflowFixture.Actor(f.Creator)).ExecuteAsync(
            NewRequest(assignees: [f.Alice.Id]),
            [],
            CancellationToken.None);

        Assert.Equal(WorkflowErrors.UnknownAssignees, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Create_StoresAttachmentsAndLinksThem()
    {
        var f = new WorkflowFixture();

        var result = await Creator(f, WorkflowFixture.Actor(f.Creator)).ExecuteAsync(
            NewRequest(),
            [WorkflowFixture.Upload("Plan v1.PDF"), WorkflowFixture.Upload("notes")],
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var stored = Assert.Single(f.Workflows.Stored);
        Assert.Equal(2, stored.Attachments.Count);
        Assert.Equal(2, f.Storage.StoredNames.Count);
        Assert.Contains(stored.Attachments, attachment => attachment.OriginalName == "Plan v1.PDF" && attachment.StoredName.EndsWith(".pdf"));
        Assert.Contains(stored.Attachments, attachment => attachment.OriginalName == "notes" && !attachment.StoredName.Contains('.'));
    }

    [Fact]
    public async Task Create_WithMoreThanFiveAttachments_Fails()
    {
        var f = new WorkflowFixture();
        var uploads = Enumerable.Range(0, 6).Select(index => WorkflowFixture.Upload($"file{index}.txt")).ToList();

        var result = await Creator(f, WorkflowFixture.Actor(f.Creator)).ExecuteAsync(NewRequest(), uploads, CancellationToken.None);

        Assert.Equal(WorkflowErrors.TooManyAttachments, Assert.Single(result.Errors).Message);
        Assert.Empty(f.Storage.StoredNames);
    }

    [Fact]
    public async Task Create_WithOversizedAttachment_Fails()
    {
        var f = new WorkflowFixture();
        var huge = new WorkflowAttachmentUpload("big.bin", "application/octet-stream", WorkflowErrors.MaxAttachmentBytes + 1, new MemoryStream());

        var result = await Creator(f, WorkflowFixture.Actor(f.Creator)).ExecuteAsync(NewRequest(), [huge], CancellationToken.None);

        Assert.Equal(WorkflowErrors.AttachmentTooLarge("big.bin"), Assert.Single(result.Errors).Message);
        Assert.Empty(f.Workflows.Stored);
    }

    [Fact]
    public async Task Create_WithEmptyAttachment_Fails()
    {
        var f = new WorkflowFixture();
        var empty = new WorkflowAttachmentUpload("empty.txt", "text/plain", 0, new MemoryStream());

        var result = await Creator(f, WorkflowFixture.Actor(f.Creator)).ExecuteAsync(NewRequest(), [empty], CancellationToken.None);

        Assert.Equal(WorkflowErrors.AttachmentEmpty, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Create_WhenNothingIsPersisted_DiscardsTheStoredFiles()
    {
        var f = new WorkflowFixture();
        var failing = new FailingSaveWorkflowRepository();

        var result = await new CreateWorkflowUseCase(failing, f.Users, f.Storage, WorkflowFixture.Actor(f.Creator)).ExecuteAsync(
            NewRequest(),
            [WorkflowFixture.Upload()],
            CancellationToken.None);

        Assert.Equal(WorkflowErrors.NothingPersisted, Assert.Single(result.Errors).Message);
        Assert.Empty(f.Storage.StoredNames);
        Assert.Single(f.Storage.Deleted);
    }

    [Fact]
    public async Task Create_WithoutASession_Fails()
    {
        var f = new WorkflowFixture();

        var result = await Creator(f, FakeCurrentActor.Anonymous()).ExecuteAsync(NewRequest(), [], CancellationToken.None);

        Assert.True(result.HasError<CredentialRejectedError>());
    }

    [Fact]
    public async Task List_FiltersByRelationshipToTheActor()
    {
        var f = new WorkflowFixture();
        f.AddWorkflow("Mine", [f.Alice]);
        f.AddWorkflow("Alice's", [f.Creator], f.Alice);
        f.AddWorkflow("Unrelated", [f.Bob], f.Bob);

        var list = new ListWorkflowsUseCase(f.Workflows, WorkflowFixture.Actor(f.Alice));

        var all = await list.ExecuteAsync(new WorkflowsQuery { Filter = WorkflowListFilter.All }, CancellationToken.None);
        var mine = await list.ExecuteAsync(new WorkflowsQuery { Filter = WorkflowListFilter.Mine }, CancellationToken.None);
        var participating = await list.ExecuteAsync(new WorkflowsQuery { Filter = WorkflowListFilter.Participating }, CancellationToken.None);

        Assert.Equal(3, all.Value.TotalCount);
        Assert.Equal(["Alice's"], mine.Value.Items.Select(item => item.Title));
        Assert.Equal(["Alice's", "Mine"], participating.Value.Items.Select(item => item.Title).Order());
    }

    [Fact]
    public async Task List_SearchesTitleAndDescriptionAndPages()
    {
        var f = new WorkflowFixture();
        f.AddWorkflow("Control plan A");
        f.AddWorkflow("Control plan B");
        f.AddWorkflow("Packaging");

        var list = new ListWorkflowsUseCase(f.Workflows, WorkflowFixture.Actor(f.Creator));

        var found = await list.ExecuteAsync(new WorkflowsQuery { Search = " control ", PageSize = 1 }, CancellationToken.None);
        var second = await list.ExecuteAsync(new WorkflowsQuery { Search = "control", Page = 1, PageSize = 1 }, CancellationToken.None);

        Assert.Equal(2, found.Value.TotalCount);
        Assert.Single(found.Value.Items);
        Assert.Single(second.Value.Items);
        Assert.NotEqual(found.Value.Items[0].Id, second.Value.Items[0].Id);
    }

    [Fact]
    public async Task Stats_CountsEachStatus()
    {
        var f = new WorkflowFixture();
        f.AddWorkflow("Open one");
        f.AddWorkflow("Open two");
        f.AddWorkflow("Solo", [f.Creator]);

        var result = await new GetWorkflowStatsUseCase(f.Workflows).ExecuteAsync(CancellationToken.None);

        Assert.Equal(3, result.Value.Total);
        Assert.Equal(2, result.Value.InProgress);
        Assert.Equal(1, result.Value.Approved);
        Assert.Equal(0, result.Value.Rejected);
    }

    [Fact]
    public async Task Users_ListsOnlyActiveOnesByName()
    {
        var f = new WorkflowFixture();
        f.Carol.Deactivate(modifiedBy: 1);

        var result = await new ListWorkflowUsersUseCase(f.Users).ExecuteAsync(CancellationToken.None);

        Assert.Equal(["Alice Assignee", "Bob Assignee", "Creator One"], result.Value.Select(user => user.Name));
    }

    [Fact]
    public async Task Get_UnknownWorkflow_NotFound()
    {
        var f = new WorkflowFixture();

        var result = await new GetWorkflowUseCase(f.Workflows, f.Users, WorkflowFixture.Actor(f.Creator))
            .ExecuteAsync(404, CancellationToken.None);

        Assert.True(result.HasError<NotFoundError>());
    }

    [Fact]
    public async Task Get_ComputesWhatEachPersonCanDo()
    {
        var f = new WorkflowFixture();
        var workflow = f.AddWorkflow();

        var asCreator = (await new GetWorkflowUseCase(f.Workflows, f.Users, WorkflowFixture.Actor(f.Creator))
            .ExecuteAsync(workflow.Id, CancellationToken.None)).Value;
        var asAlice = (await new GetWorkflowUseCase(f.Workflows, f.Users, WorkflowFixture.Actor(f.Alice))
            .ExecuteAsync(workflow.Id, CancellationToken.None)).Value;
        var asCarol = (await new GetWorkflowUseCase(f.Workflows, f.Users, WorkflowFixture.Actor(f.Carol))
            .ExecuteAsync(workflow.Id, CancellationToken.None)).Value;

        Assert.True(asCreator.Permissions.IsCreator);
        Assert.True(asCreator.Permissions.CanEdit);
        Assert.True(asCreator.Permissions.CanManageAssignees);
        Assert.False(asCreator.Permissions.CanApprove);

        Assert.True(asAlice.Permissions.IsAssignee);
        Assert.True(asAlice.Permissions.CanApprove);
        Assert.True(asAlice.Permissions.CanReject);
        Assert.False(asAlice.Permissions.CanEdit);
        Assert.False(asAlice.Permissions.CanManageAssignees);

        Assert.False(asCarol.Permissions.IsAssignee);
        Assert.False(asCarol.Permissions.CanApprove);
        Assert.False(asCarol.Permissions.CanEdit);
    }

    [Fact]
    public async Task Get_ReturnsPeopleHistoryAndAssigneeApprovals()
    {
        var f = new WorkflowFixture();
        var workflow = f.AddWorkflow();

        var detail = (await new GetWorkflowUseCase(f.Workflows, f.Users, WorkflowFixture.Actor(f.Creator))
            .ExecuteAsync(workflow.Id, CancellationToken.None)).Value;

        Assert.Equal("Creator One", detail.CreatorName);
        Assert.Equal(3, detail.Assignees.Count);
        Assert.Equal(["Creator One"], detail.Assignees.Where(assignee => assignee.Approved).Select(assignee => assignee.Name));
        Assert.Equal(WorkflowAction.Approval, detail.Steps[0].Action);
        Assert.Equal("Creator One", detail.Steps[0].UserName);
    }

    [Fact]
    public async Task Get_KeepsTheNamesOfDeactivatedPeopleInTheHistory()
    {
        var f = new WorkflowFixture();
        var workflow = f.AddWorkflow();
        f.Alice.Deactivate(modifiedBy: 1);

        var detail = (await new GetWorkflowUseCase(f.Workflows, f.Users, WorkflowFixture.Actor(f.Creator))
            .ExecuteAsync(workflow.Id, CancellationToken.None)).Value;

        Assert.Contains(detail.Assignees, assignee => assignee.Name == "Alice Assignee");
    }

    [Fact]
    public async Task Update_ByTheCreator_ChangesTheWorkflow()
    {
        var f = new WorkflowFixture();
        var workflow = f.AddWorkflow();

        var result = await new UpdateWorkflowUseCase(f.Workflows, WorkflowFixture.Actor(f.Creator)).ExecuteAsync(
            workflow.Id,
            new UpdateWorkflowRequest("New title", null, WorkflowPriority.Low, null),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("New title", workflow.Title);
        Assert.Equal(string.Empty, workflow.Description);
        Assert.Equal(WorkflowAction.Edited, workflow.Steps.Last().Action);
    }

    [Fact]
    public async Task Update_ByAnAssignee_IsForbidden()
    {
        var f = new WorkflowFixture();
        var workflow = f.AddWorkflow();

        var result = await new UpdateWorkflowUseCase(f.Workflows, WorkflowFixture.Actor(f.Alice)).ExecuteAsync(
            workflow.Id,
            new UpdateWorkflowRequest("Hijack", "x", WorkflowPriority.Low, null),
            CancellationToken.None);

        Assert.True(result.HasError<ForbiddenError>());
        Assert.NotEqual("Hijack", workflow.Title);
    }

    [Fact]
    public async Task Update_WithoutChanges_SucceedsWithoutSaving()
    {
        var f = new WorkflowFixture();
        var workflow = f.AddWorkflow();
        var saves = f.Workflows.SaveCount;

        var result = await new UpdateWorkflowUseCase(f.Workflows, WorkflowFixture.Actor(f.Creator)).ExecuteAsync(
            workflow.Id,
            new UpdateWorkflowRequest(workflow.Title, workflow.Description, workflow.Priority, workflow.Category),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(saves, f.Workflows.SaveCount);
    }

    [Fact]
    public async Task Update_UnknownWorkflow_NotFound()
    {
        var f = new WorkflowFixture();

        var result = await new UpdateWorkflowUseCase(f.Workflows, WorkflowFixture.Actor(f.Creator)).ExecuteAsync(
            404,
            new UpdateWorkflowRequest("Title", "x", WorkflowPriority.Low, null),
            CancellationToken.None);

        Assert.True(result.HasError<NotFoundError>());
    }

    [Fact]
    public async Task Approve_ReportsWhenItIsStillWaitingForOthers()
    {
        var f = new WorkflowFixture();
        var workflow = f.AddWorkflow();

        var result = await new ApproveWorkflowUseCase(f.Workflows, WorkflowFixture.Actor(f.Alice))
            .ExecuteAsync(workflow.Id, Justify(), CancellationToken.None);

        Assert.Equal(WorkflowOutcomeTexts.ApprovalRecorded, result.Value.Message);
        Assert.Equal(WorkflowStatus.InProgress, result.Value.Status);
    }

    [Fact]
    public async Task Approve_ByTheLastAssignee_ClosesTheWorkflow()
    {
        var f = new WorkflowFixture();
        var workflow = f.AddWorkflow();

        await new ApproveWorkflowUseCase(f.Workflows, WorkflowFixture.Actor(f.Alice))
            .ExecuteAsync(workflow.Id, Justify(), CancellationToken.None);

        var result = await new ApproveWorkflowUseCase(f.Workflows, WorkflowFixture.Actor(f.Bob))
            .ExecuteAsync(workflow.Id, Justify(), CancellationToken.None);

        Assert.Equal(WorkflowOutcomeTexts.WorkflowApproved, result.Value.Message);
        Assert.Equal(WorkflowStatus.Approved, result.Value.Status);
        Assert.Equal(WorkflowStatus.Approved, workflow.Status);
    }

    [Fact]
    public async Task Approve_ByAnOutsider_IsForbidden()
    {
        var f = new WorkflowFixture();
        var workflow = f.AddWorkflow();

        var result = await new ApproveWorkflowUseCase(f.Workflows, WorkflowFixture.Actor(f.Carol))
            .ExecuteAsync(workflow.Id, Justify(), CancellationToken.None);

        Assert.True(result.HasError<ForbiddenError>());
    }

    [Fact]
    public async Task Approve_Twice_Fails()
    {
        var f = new WorkflowFixture();
        var workflow = f.AddWorkflow();
        var approve = new ApproveWorkflowUseCase(f.Workflows, WorkflowFixture.Actor(f.Alice));

        await approve.ExecuteAsync(workflow.Id, Justify(), CancellationToken.None);
        var second = await approve.ExecuteAsync(workflow.Id, Justify(), CancellationToken.None);

        Assert.Equal(WorkflowErrors.AlreadyApproved, Assert.Single(second.Errors).Message);
    }

    [Fact]
    public async Task Reject_OpensAReviewForTheCreator()
    {
        var f = new WorkflowFixture();
        var workflow = f.AddWorkflow();

        var result = await new RejectWorkflowUseCase(f.Workflows, WorkflowFixture.Actor(f.Alice))
            .ExecuteAsync(workflow.Id, Justify("Wrong tolerance"), CancellationToken.None);

        Assert.Equal(WorkflowOutcomeTexts.RejectionRecorded, result.Value.Message);
        Assert.True(workflow.HasRejectionAwaitingCreator);
    }

    [Fact]
    public async Task Reject_ByTheCreator_Fails()
    {
        var f = new WorkflowFixture();
        var workflow = f.AddWorkflow();

        var result = await new RejectWorkflowUseCase(f.Workflows, WorkflowFixture.Actor(f.Creator))
            .ExecuteAsync(workflow.Id, Justify(), CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.False(workflow.HasRejectionAwaitingCreator);
    }

    [Fact]
    public async Task RejectionNegotiation_ReturnsAndEndsInApproval()
    {
        var f = new WorkflowFixture();
        var workflow = f.AddWorkflow("Negotiated", [f.Alice]);

        await new RejectWorkflowUseCase(f.Workflows, WorkflowFixture.Actor(f.Alice))
            .ExecuteAsync(workflow.Id, Justify("Wrong tolerance"), CancellationToken.None);

        var step = workflow.Steps.Single(candidate => candidate.Action == WorkflowAction.Rejection);

        var returned = await new ReturnRejectionToAssigneeUseCase(f.Workflows, WorkflowFixture.Actor(f.Creator))
            .ExecuteAsync(workflow.Id, step.Id, Justify("It is within spec"), CancellationToken.None);

        Assert.True(returned.IsSuccess);
        Assert.Equal(ReviewStage.AssigneeReview, step.Review);

        var back = await new ReturnRejectionToCreatorUseCase(f.Workflows, WorkflowFixture.Actor(f.Alice))
            .ExecuteAsync(workflow.Id, step.Id, Justify("Check the drawing"), CancellationToken.None);

        Assert.True(back.IsSuccess);
        Assert.Equal(ReviewStage.CreatorReview, step.Review);

        await new ReturnRejectionToAssigneeUseCase(f.Workflows, WorkflowFixture.Actor(f.Creator))
            .ExecuteAsync(workflow.Id, step.Id, Justify("Checked, it is fine"), CancellationToken.None);

        var approved = await new ApproveWorkflowUseCase(f.Workflows, WorkflowFixture.Actor(f.Alice))
            .ExecuteAsync(workflow.Id, Justify("Convinced"), CancellationToken.None);

        Assert.Equal(WorkflowStatus.Approved, approved.Value.Status);
        Assert.Equal(ReviewStage.None, step.Review);

        var messages = await new ListWorkflowMessagesUseCase(f.Workflows, f.Users, WorkflowFixture.Actor(f.Alice))
            .ExecuteAsync(workflow.Id, step.Id, CancellationToken.None);

        Assert.Equal(
            ["It is within spec", "Check the drawing", "Checked, it is fine"],
            messages.Value.Select(message => message.Text));
        Assert.Equal(
            ["Creator One", "Alice Assignee", "Creator One"],
            messages.Value.Select(message => message.UserName));
    }

    [Fact]
    public async Task ReturnRejection_ByTheWrongPerson_IsForbidden()
    {
        var f = new WorkflowFixture();
        var workflow = f.AddWorkflow();

        await new RejectWorkflowUseCase(f.Workflows, WorkflowFixture.Actor(f.Alice))
            .ExecuteAsync(workflow.Id, Justify(), CancellationToken.None);

        var step = workflow.Steps.Single(candidate => candidate.Action == WorkflowAction.Rejection);

        var byAssignee = await new ReturnRejectionToAssigneeUseCase(f.Workflows, WorkflowFixture.Actor(f.Bob))
            .ExecuteAsync(workflow.Id, step.Id, Justify(), CancellationToken.None);

        Assert.True(byAssignee.HasError<ForbiddenError>());
    }

    [Fact]
    public async Task ReturnRejection_WithUnknownStep_NotFound()
    {
        var f = new WorkflowFixture();
        var workflow = f.AddWorkflow();

        var result = await new ReturnRejectionToAssigneeUseCase(f.Workflows, WorkflowFixture.Actor(f.Creator))
            .ExecuteAsync(workflow.Id, 4040, Justify(), CancellationToken.None);

        Assert.True(result.HasError<NotFoundError>());
    }

    [Fact]
    public async Task AcceptRejection_RejectsTheWorkflow()
    {
        var f = new WorkflowFixture();
        var workflow = f.AddWorkflow();

        await new RejectWorkflowUseCase(f.Workflows, WorkflowFixture.Actor(f.Alice))
            .ExecuteAsync(workflow.Id, Justify("Wrong tolerance"), CancellationToken.None);

        var result = await new AcceptWorkflowRejectionUseCase(f.Workflows, WorkflowFixture.Actor(f.Creator))
            .ExecuteAsync(workflow.Id, Justify("Agreed"), CancellationToken.None);

        Assert.Equal(WorkflowStatus.Rejected, result.Value.Status);
        Assert.Equal(WorkflowOutcomeTexts.WorkflowRejected, result.Value.Message);
        Assert.False(workflow.HasRejectionAwaitingCreator);
    }

    [Fact]
    public async Task AcceptRejection_ByAnAssignee_IsForbidden()
    {
        var f = new WorkflowFixture();
        var workflow = f.AddWorkflow();

        await new RejectWorkflowUseCase(f.Workflows, WorkflowFixture.Actor(f.Alice))
            .ExecuteAsync(workflow.Id, Justify(), CancellationToken.None);

        var result = await new AcceptWorkflowRejectionUseCase(f.Workflows, WorkflowFixture.Actor(f.Bob))
            .ExecuteAsync(workflow.Id, Justify(), CancellationToken.None);

        Assert.True(result.HasError<ForbiddenError>());
        Assert.Equal(WorkflowStatus.InProgress, workflow.Status);
    }

    [Fact]
    public async Task AddAssignee_LinksTheUserAndNamesThemInTheHistory()
    {
        var f = new WorkflowFixture();
        var workflow = f.AddWorkflow("Add", [f.Alice]);

        var result = await new AddWorkflowAssigneeUseCase(f.Workflows, f.Users, WorkflowFixture.Actor(f.Creator))
            .ExecuteAsync(workflow.Id, new AddWorkflowAssigneeRequest(f.Carol.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Carol Outsider", result.Value.Name);
        Assert.False(result.Value.Approved);
        Assert.True(workflow.IsAssignee(f.Carol.Id));
        Assert.Equal("Responsável adicionado: Carol Outsider (1004)", workflow.Steps.Last().Description);
    }

    [Fact]
    public async Task AddAssignee_UnknownUser_NotFound()
    {
        var f = new WorkflowFixture();
        var workflow = f.AddWorkflow();

        var result = await new AddWorkflowAssigneeUseCase(f.Workflows, f.Users, WorkflowFixture.Actor(f.Creator))
            .ExecuteAsync(workflow.Id, new AddWorkflowAssigneeRequest(999), CancellationToken.None);

        Assert.True(result.HasError<NotFoundError>());
    }

    [Fact]
    public async Task AddAssignee_ByAnAssignee_IsForbidden()
    {
        var f = new WorkflowFixture();
        var workflow = f.AddWorkflow();

        var result = await new AddWorkflowAssigneeUseCase(f.Workflows, f.Users, WorkflowFixture.Actor(f.Alice))
            .ExecuteAsync(workflow.Id, new AddWorkflowAssigneeRequest(f.Carol.Id), CancellationToken.None);

        Assert.True(result.HasError<ForbiddenError>());
    }

    [Fact]
    public async Task AddAssignee_AlreadyListed_Fails()
    {
        var f = new WorkflowFixture();
        var workflow = f.AddWorkflow();

        var result = await new AddWorkflowAssigneeUseCase(f.Workflows, f.Users, WorkflowFixture.Actor(f.Creator))
            .ExecuteAsync(workflow.Id, new AddWorkflowAssigneeRequest(f.Alice.Id), CancellationToken.None);

        Assert.Equal(WorkflowErrors.AlreadyAssignee, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task ReplaceAssignee_SwapsAndNamesBothInTheHistory()
    {
        var f = new WorkflowFixture();
        var workflow = f.AddWorkflow("Replace", [f.Alice]);

        var result = await new ReplaceWorkflowAssigneeUseCase(f.Workflows, f.Users, WorkflowFixture.Actor(f.Creator))
            .ExecuteAsync(workflow.Id, f.Alice.Id, new ReplaceWorkflowAssigneeRequest(f.Carol.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(WorkflowOutcomeTexts.AssigneeReplaced, result.Value.Message);
        Assert.False(workflow.IsAssignee(f.Alice.Id));
        Assert.True(workflow.IsAssignee(f.Carol.Id));
        Assert.Equal(
            "Substituição de responsável: Alice Assignee (1002) → Carol Outsider (1004)",
            workflow.Steps.Last().Description);
    }

    [Fact]
    public async Task ReplaceAssignee_WithADeactivatedReplacement_NotFound()
    {
        var f = new WorkflowFixture();
        var workflow = f.AddWorkflow("Replace", [f.Alice]);
        f.Carol.Deactivate(modifiedBy: 1);

        var result = await new ReplaceWorkflowAssigneeUseCase(f.Workflows, f.Users, WorkflowFixture.Actor(f.Creator))
            .ExecuteAsync(workflow.Id, f.Alice.Id, new ReplaceWorkflowAssigneeRequest(f.Carol.Id), CancellationToken.None);

        Assert.True(result.HasError<NotFoundError>());
        Assert.True(workflow.IsAssignee(f.Alice.Id));
    }

    [Fact]
    public async Task ReplaceAssignee_WhoAlreadyApproved_Fails()
    {
        var f = new WorkflowFixture();
        var workflow = f.AddWorkflow();

        await new ApproveWorkflowUseCase(f.Workflows, WorkflowFixture.Actor(f.Alice))
            .ExecuteAsync(workflow.Id, Justify(), CancellationToken.None);

        var result = await new ReplaceWorkflowAssigneeUseCase(f.Workflows, f.Users, WorkflowFixture.Actor(f.Creator))
            .ExecuteAsync(workflow.Id, f.Alice.Id, new ReplaceWorkflowAssigneeRequest(f.Carol.Id), CancellationToken.None);

        Assert.Equal(WorkflowErrors.AssigneeAlreadyApproved, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task ReplaceAssignee_WhoIsNotAssigned_NotFound()
    {
        var f = new WorkflowFixture();
        var workflow = f.AddWorkflow();

        var result = await new ReplaceWorkflowAssigneeUseCase(f.Workflows, f.Users, WorkflowFixture.Actor(f.Creator))
            .ExecuteAsync(workflow.Id, f.Carol.Id, new ReplaceWorkflowAssigneeRequest(f.Bob.Id), CancellationToken.None);

        Assert.True(result.HasError<NotFoundError>());
    }

    [Fact]
    public async Task Messages_UnknownStep_NotFound()
    {
        var f = new WorkflowFixture();
        var workflow = f.AddWorkflow();

        var result = await new ListWorkflowMessagesUseCase(f.Workflows, f.Users, WorkflowFixture.Actor(f.Alice))
            .ExecuteAsync(workflow.Id, 4040, CancellationToken.None);

        Assert.True(result.HasError<NotFoundError>());
    }

    [Fact]
    public async Task Attachment_DownloadsTheStoredFile()
    {
        var f = new WorkflowFixture();
        var created = await Creator(f, WorkflowFixture.Actor(f.Creator)).ExecuteAsync(
            NewRequest(),
            [WorkflowFixture.Upload("plan.pdf", 32)],
            CancellationToken.None);

        var attachment = f.Workflows.Stored.Single().Attachments.Single();

        var result = await new GetWorkflowAttachmentUseCase(f.Workflows, f.Storage)
            .ExecuteAsync(created.Value.Id, attachment.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("plan.pdf", result.Value.FileName);
        Assert.Equal("application/pdf", result.Value.ContentType);
        using var buffer = new MemoryStream();
        await result.Value.Content.CopyToAsync(buffer);
        Assert.Equal(32, buffer.Length);
    }

    [Fact]
    public async Task Attachment_UnknownIdOrMissingFile_NotFound()
    {
        var f = new WorkflowFixture();
        await Creator(f, WorkflowFixture.Actor(f.Creator)).ExecuteAsync(
            NewRequest(),
            [WorkflowFixture.Upload()],
            CancellationToken.None);

        var workflow = f.Workflows.Stored.Single();
        var attachment = workflow.Attachments.Single();
        var download = new GetWorkflowAttachmentUseCase(f.Workflows, f.Storage);

        var unknown = await download.ExecuteAsync(workflow.Id, 4040, CancellationToken.None);
        Assert.True(unknown.HasError<NotFoundError>());

        await f.Storage.DeleteAsync(attachment.StoredName, CancellationToken.None);
        var missing = await download.ExecuteAsync(workflow.Id, attachment.Id, CancellationToken.None);
        Assert.True(missing.HasError<NotFoundError>());
    }

    private sealed class FailingSaveWorkflowRepository : MeOne.Tests.Fakes.FakeWorkflowRepository
    {
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken) => Task.FromResult(0);
    }
}
