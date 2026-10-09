using IrisWeb.Domain.Entities;
using IrisWeb.Domain.Enums;
using IrisWeb.Domain.Errors;

namespace IrisWeb.Tests.Domain;

public sealed class ModelVersionTests
{
    [Fact]
    public void NewVersionStartsAsDraft()
    {
        var version = CreateVersion();

        Assert.Equal(ModelVersionStatus.Draft, version.Status);
    }

    [Fact]
    public void UploadAndApprovalFollowExpectedLifecycle()
    {
        var version = CreateVersion();

        version.BeginUpload("fpc-detector/1.0.0/model-package.zip");
        version.MarkCandidate(1024, new string('a', 64), "{\"runtime\":\"onnxruntime\"}");
        version.Approve("123456");

        Assert.Equal(ModelVersionStatus.Approved, version.Status);
        Assert.Equal("123456", version.ApprovedBy);
        Assert.NotNull(version.ApprovedAtUtc);
    }

    [Fact]
    public void CannotApproveBeforeUploadIsCompleted()
    {
        var version = CreateVersion();
        var action = () => version.Approve("123456");
        Assert.Throws<DomainRuleException>(action);
    }

    [Fact]
    public void CannotArchiveWhileUploading()
    {
        var version = CreateVersion();
        version.BeginUpload("fpc-detector/1.0.0/model-package.zip");

        var action = version.Archive;

        Assert.Throws<DomainRuleException>(action);
    }

    private static ModelVersion CreateVersion() =>
        ModelVersion.Create(Guid.NewGuid(), "1.0.0", "onnxruntime", "123456");
}
