using MeOne.Application.Communication.Workflows.Dtos;
using MeOne.Domain.Entities;
using MeOne.Domain.Enums;

namespace MeOne.Tests.Fakes;

public sealed class WorkflowFixture
{
    public WorkflowFixture()
    {
        Access = new AccessFixture();

        Creator = AddUser(1001, "Creator One");
        Alice = AddUser(1002, "Alice Assignee");
        Bob = AddUser(1003, "Bob Assignee");
        Carol = AddUser(1004, "Carol Outsider");
    }

    public AccessFixture Access { get; }

    public FakeWorkflowRepository Workflows { get; } = new();

    public FakeWorkflowFileStorage Storage { get; } = new();

    public User Creator { get; }

    public User Alice { get; }

    public User Bob { get; }

    public User Carol { get; }

    public FakeUserRepository Users => Access.Users;

    public static FakeCurrentActor Actor(User user) => FakeCurrentActor.For(user);

    public User AddUser(uint registration, string name) =>
        Access.AddUser(registration, $"user{registration}@example.com", name);

    public Workflow AddWorkflow(
        string title = "Review the control plan",
        IEnumerable<User>? assignees = null,
        User? creator = null,
        DateOnly? dueDate = null)
    {
        var owner = creator ?? Creator;

        var workflow = Workflow.Create(
            title,
            "Description",
            WorkflowPriority.Medium,
            WorkflowCategory.Procedures,
            dueDate ?? DateOnly.FromDateTime(DateTime.UtcNow).AddDays(7),
            owner.Id,
            (assignees ?? [Alice, Bob]).Select(user => user.Id),
            DateTime.UtcNow);

        Workflows.Insert(workflow);
        Workflows.SaveChangesAsync(CancellationToken.None).GetAwaiter().GetResult();

        return workflow;
    }

    public static WorkflowAttachmentUpload Upload(string fileName = "plan.pdf", int length = 16)
    {
        var content = new byte[length];
        Array.Fill(content, (byte)7);

        return new WorkflowAttachmentUpload(fileName, "application/pdf", length, new MemoryStream(content));
    }
}
