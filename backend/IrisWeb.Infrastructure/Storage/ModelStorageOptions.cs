namespace IrisWeb.Infrastructure.Storage;

public sealed class ModelStorageOptions
{
    public const string SectionName = "ModelStorage";

    public string Path { get; init; } = System.IO.Path.Combine("Data", "model-artifacts");
}
