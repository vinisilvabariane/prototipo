namespace MeOne.Infrastructure.Storage;

public sealed class WorkflowStorageOptions
{
    public const string SectionName = "Workflow:Storage";

    public string Path { get; set; } = "uploads/workflows";
}
