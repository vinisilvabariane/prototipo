namespace MeOne.Infrastructure.Storage;

public sealed class WorkInstructionStorageOptions
{
    public const string SectionName = "WorkInstruction:Storage";

    public string Path { get; set; } = "uploads/work-instructions";
}
