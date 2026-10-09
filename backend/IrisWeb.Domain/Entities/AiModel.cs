using IrisWeb.Domain.Enums;
using IrisWeb.Domain.Errors;

namespace IrisWeb.Domain.Entities;

public sealed class AiModel : Entity
{
    private AiModel() { }

    private AiModel(
        Guid id,
        string key,
        string name,
        string? description,
        AiTaskType taskType,
        ModelPlatform platform,
        string createdBy)
    {
        Id = id;
        Key = key;
        Name = name;
        Description = description;
        TaskType = taskType;
        Platform = platform;
        CreatedBy = createdBy;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public string Key { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public AiTaskType TaskType { get; private set; }

    public ModelPlatform Platform { get; private set; }

    public Guid? ActiveVersionId { get; private set; }

    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }
    public ICollection<ModelVersion> Versions { get; private set; } = new List<ModelVersion>();

    public bool IsActiveVersion(Guid versionId) => ActiveVersionId == versionId;

    public void SetActiveVersion(ModelVersion version)
    {
        if (version.ModelId != Id)
            throw new DomainRuleException(ModelCatalogErrors.VersionBelongsToAnotherModel);

        if (version.Status != ModelVersionStatus.Approved)
            throw new DomainRuleException(ModelCatalogErrors.ActiveVersionMustBeApproved(version.Status.ToString()));

        ActiveVersionId = version.Id;
    }

    public void ClearActiveVersion() => ActiveVersionId = null;

    public static AiModel Create(
        string key,
        string name,
        string? description,
        AiTaskType taskType,
        ModelPlatform platform,
        string createdBy) =>
        new(
            Guid.NewGuid(),
            key.Trim().ToLowerInvariant(),
            name.Trim(),
            description?.Trim(),
            taskType,
            platform,
            createdBy
        );
}
