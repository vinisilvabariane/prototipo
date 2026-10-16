using MeOne.Domain.Entities;
using MeOne.Domain.Enums;
using MeOne.Infrastructure.Data.Context;
using MeOne.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MeOne.Tests.Infrastructure;

public sealed class WorkflowMappingTests
{
    private static MeOneDbContext Context() =>
        new(new DbContextOptionsBuilder<MeOneDbContext>()
            .UseMySql("Server=localhost;Database=meone;User=x;Password=x", new MySqlServerVersion(new Version(8, 0, 0)))
            .Options);

    private static Dictionary<string, string?> Columns<T>(MeOneDbContext context, out string? table)
    {
        var entity = context.Model.FindEntityType(typeof(T))!;
        table = entity.GetTableName();
        return entity.GetProperties().ToDictionary(p => p.Name, p => (string?)p.GetColumnName());
    }

    [Fact]
    public void Workflows_UseSnakeCaseColumns()
    {
        using var context = Context();
        var columns = Columns<Workflow>(context, out var table);

        Assert.Equal("workflows", table);
        Assert.Equal("title", columns[nameof(Workflow.Title)]);
        Assert.Equal("priority", columns[nameof(Workflow.Priority)]);
        Assert.Equal("status", columns[nameof(Workflow.Status)]);
        Assert.Equal("fk_creator", columns[nameof(Workflow.CreatorId)]);
        Assert.Equal("created_at", columns[nameof(Workflow.CreatedAt)]);
        Assert.Equal("completed_at", columns[nameof(Workflow.CompletedAt)]);
        Assert.Equal("due_date", columns[nameof(Workflow.DueDate)]);
        Assert.Equal("version", columns[nameof(Workflow.Version)]);
        Assert.DoesNotContain(nameof(Workflow.IsOpen), columns.Keys);
    }

    [Fact]
    public void WorkflowChildTables_UseSnakeCaseNames()
    {
        using var context = Context();

        var assignees = Columns<WorkflowAssignee>(context, out var assigneesTable);
        Assert.Equal("workflow_assignees", assigneesTable);
        Assert.Equal("fk_workflow", assignees[nameof(WorkflowAssignee.WorkflowId)]);
        Assert.Equal("fk_user", assignees[nameof(WorkflowAssignee.UserId)]);
        Assert.Equal("assigned_at", assignees[nameof(WorkflowAssignee.AssignedAt)]);

        var steps = Columns<WorkflowStep>(context, out var stepsTable);
        Assert.Equal("workflow_steps", stepsTable);
        Assert.Equal("action", steps[nameof(WorkflowStep.Action)]);
        Assert.Equal("justification", steps[nameof(WorkflowStep.Justification)]);
        Assert.Equal("review", steps[nameof(WorkflowStep.Review)]);

        var attachments = Columns<WorkflowAttachment>(context, out var attachmentsTable);
        Assert.Equal("workflow_attachments", attachmentsTable);
        Assert.Equal("original_name", attachments[nameof(WorkflowAttachment.OriginalName)]);
        Assert.Equal("stored_name", attachments[nameof(WorkflowAttachment.StoredName)]);
        Assert.Equal("content_type", attachments[nameof(WorkflowAttachment.ContentType)]);

        var messages = Columns<WorkflowMessage>(context, out var messagesTable);
        Assert.Equal("workflow_messages", messagesTable);
        Assert.Equal("fk_step", messages[nameof(WorkflowMessage.StepId)]);
        Assert.Equal("text", messages[nameof(WorkflowMessage.Text)]);
    }

    [Fact]
    public void Version_IsTheConcurrencyToken()
    {
        using var context = Context();
        var entity = context.Model.FindEntityType(typeof(Workflow))!;

        Assert.True(entity.FindProperty(nameof(Workflow.Version))!.IsConcurrencyToken);
    }

    [Fact]
    public void EnumsAreStoredAsText()
    {
        using var context = Context();
        var entity = context.Model.FindEntityType(typeof(Workflow))!;

        Assert.Equal(typeof(string), entity.FindProperty(nameof(Workflow.Status))!.GetProviderClrType());
        Assert.Equal(typeof(string), entity.FindProperty(nameof(Workflow.Priority))!.GetProviderClrType());
        Assert.Equal(typeof(string), entity.FindProperty(nameof(Workflow.Category))!.GetProviderClrType());
    }

    [Fact]
    public void Timestamps_ComeBackAsUtc()
    {
        using var context = Context();
        var property = context.Model.FindEntityType(typeof(Workflow))!.FindProperty(nameof(Workflow.CreatedAt))!;
        var converter = property.GetValueConverter()!;

        var read = (DateTime)converter.ConvertFromProvider(new DateTime(2026, 10, 15, 12, 0, 0, DateTimeKind.Unspecified))!;

        Assert.Equal(DateTimeKind.Utc, read.Kind);
    }

    [Theory]
    [InlineData(WorkflowListFilter.All)]
    [InlineData(WorkflowListFilter.Mine)]
    [InlineData(WorkflowListFilter.Participating)]
    public void ListQuery_TranslatesEveryFilter(WorkflowListFilter filter)
    {
        using var context = Context();

        var sql = WorkflowRepository.ApplyFilter(context.Set<Workflow>(), filter, actorId: 7, search: "plan").ToQueryString();

        Assert.Contains("workflows", sql);
        Assert.Contains("LIKE", sql);

        if (filter == WorkflowListFilter.Mine)
            Assert.Contains("fk_creator", sql);

        if (filter == WorkflowListFilter.Participating)
            Assert.Contains("workflow_assignees", sql);
    }
}
