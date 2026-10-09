using IrisWeb.Domain.Enums;
using IrisWeb.Domain.Errors;

namespace IrisWeb.Domain.Entities;

public sealed class ModelVersion : Entity
{
    private ModelVersion() { }

    private ModelVersion(Guid id, Guid modelId, string version, string runtime, string createdBy)
    {
        Id = id;
        ModelId = modelId;
        Version = version;
        Runtime = runtime;
        CreatedBy = createdBy;
        Status = ModelVersionStatus.Draft;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid ModelId { get; private set; }
    public AiModel Model { get; private set; } = null!;
    public string Version { get; private set; } = string.Empty;
    public string Runtime { get; private set; } = string.Empty;
    public ModelVersionStatus Status { get; private set; }
    public string? ArtifactObjectKey { get; private set; }
    public long? ArtifactSize { get; private set; }
    public string? Sha256 { get; private set; }
    public string? ManifestJson { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public string? ApprovedBy { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ApprovedAtUtc { get; private set; }

    public static ModelVersion Create(
        Guid modelId,
        string version,
        string runtime,
        string createdBy) =>
        new(
            Guid.NewGuid(),
            modelId,
            version.Trim(),
            runtime.Trim().ToLowerInvariant(),
            createdBy
        );

    public bool CanBeginUpload => Status is ModelVersionStatus.Draft;

    public bool CanMarkCandidate => Status is ModelVersionStatus.Uploading;

    public bool CanApprove => Status is ModelVersionStatus.Candidate;

    public bool CanArchive => Status is not ModelVersionStatus.Uploading;

    public bool CanUnarchive => Status is ModelVersionStatus.Archived;

    public void BeginUpload(string objectKey)
    {
        EnsureStatus(ModelVersionStatus.Draft);
        ArtifactObjectKey = objectKey;
        Status = ModelVersionStatus.Uploading;
    }

    public void MarkCandidate(long artifactSize, string sha256, string manifestJson)
    {
        EnsureStatus(ModelVersionStatus.Uploading);
        ArtifactSize = artifactSize;
        Sha256 = sha256.ToLowerInvariant();
        ManifestJson = manifestJson;
        Status = ModelVersionStatus.Candidate;
    }

    public void Approve(string approvedBy)
    {
        EnsureStatus(ModelVersionStatus.Candidate);
        ApprovedBy = approvedBy;
        ApprovedAtUtc = DateTime.UtcNow;
        Status = ModelVersionStatus.Approved;
    }

    public void Archive()
    {
        if (Status is ModelVersionStatus.Uploading)
            throw new DomainRuleException(ModelCatalogErrors.CannotArchiveWhileUploading);
        if (Status is ModelVersionStatus.Archived)
            return;
        Status = ModelVersionStatus.Archived;
    }

    public void Unarchive()
    {
        EnsureStatus(ModelVersionStatus.Archived);

        Status = ApprovedAtUtc is not null
            ? ModelVersionStatus.Approved
            : Sha256 is not null
                ? ModelVersionStatus.Candidate
                : ModelVersionStatus.Draft;
    }

    private void EnsureStatus(ModelVersionStatus expected)
    {
        if (Status != expected)
            throw new DomainRuleException(ModelCatalogErrors.InvalidTransition(expected.ToString(), Status.ToString()));
    }
}
