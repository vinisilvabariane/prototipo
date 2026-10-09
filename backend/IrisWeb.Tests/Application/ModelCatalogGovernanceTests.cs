using System.Text.Json;
using IrisWeb.Application.Communication.ModelCatalog.Requests;
using IrisWeb.Application.Errors;
using IrisWeb.Application.UseCases.ModelCatalog;
using IrisWeb.Domain.Entities;
using IrisWeb.Domain.Enums;
using IrisWeb.Domain.Errors;
using IrisWeb.Tests.Fakes;

namespace IrisWeb.Tests.Application;

public sealed class ModelCatalogGovernanceTests
{
    private static string Manifest() =>
        JsonSerializer.Deserialize<JsonElement>("""{"entrypoint":"model.onnx"}""").GetRawText();

    private static (FakeModelCatalogRepository Repository, AiModel Model) CatalogoCom()
    {
        var repository = new FakeModelCatalogRepository();
        var model = AiModel.Create("facenet-embeddings-mobile", "FaceNet mobile", null,
            AiTaskType.FaceRecognition, ModelPlatform.Mobile, "vinicius");
        repository.Seed(model);
        return (repository, model);
    }

    private static ModelVersion Versao(Guid modelId, string version, ModelVersionStatus ate)
    {
        var item = ModelVersion.Create(modelId, version, "onnxruntime-web", "vinicius");
        if (ate == ModelVersionStatus.Draft) return item;

        item.BeginUpload($"facenet-embeddings-mobile/{version}/model-package.zip");
        if (ate == ModelVersionStatus.Uploading) return item;

        item.MarkCandidate(1024, "ABC123", Manifest());
        if (ate == ModelVersionStatus.Candidate) return item;

        item.Approve("vinicius");
        if (ate == ModelVersionStatus.Approved) return item;

        item.Archive();
        return item;
    }

    [Fact]
    public void Unarchive_VersaoQueJaFoiAprovada_VoltaParaApproved_ComAutoriaPreservada()
    {
        var version = Versao(Guid.NewGuid(), "1.1.1", ModelVersionStatus.Archived);
        var approvedBy = version.ApprovedBy;
        var approvedAt = version.ApprovedAtUtc;

        version.Unarchive();

        Assert.Equal(ModelVersionStatus.Approved, version.Status);
        Assert.Equal(approvedBy, version.ApprovedBy);
        Assert.Equal(approvedAt, version.ApprovedAtUtc);
    }

    [Fact]
    public void Unarchive_VersaoCandidataArquivada_VoltaParaCandidate()
    {
        var version = Versao(Guid.NewGuid(), "1.0.0", ModelVersionStatus.Candidate);
        version.Archive();

        version.Unarchive();

        Assert.Equal(ModelVersionStatus.Candidate, version.Status);
    }

    [Fact]
    public void Unarchive_VersaoSemUploadConcluido_VoltaParaDraft()
    {
        var version = Versao(Guid.NewGuid(), "1.0.0", ModelVersionStatus.Draft);
        version.Archive();

        version.Unarchive();

        Assert.Equal(ModelVersionStatus.Draft, version.Status);
    }

    [Theory]
    [InlineData(ModelVersionStatus.Draft)]
    [InlineData(ModelVersionStatus.Candidate)]
    [InlineData(ModelVersionStatus.Approved)]
    public void Unarchive_ForaDeArchived_Recusa(ModelVersionStatus status)
    {
        var version = Versao(Guid.NewGuid(), "1.0.0", status);

        Assert.False(version.CanUnarchive);
        Assert.Throws<DomainRuleException>(version.Unarchive);
    }

    [Fact]
    public void SetActiveVersion_SoAceitaAprovadaDoProprioModelo()
    {
        var model = AiModel.Create("k", "K", null, AiTaskType.FaceRecognition, ModelPlatform.Mobile, "v");
        var aprovada = Versao(model.Id, "1.0.1", ModelVersionStatus.Approved);
        var candidata = Versao(model.Id, "1.1.0", ModelVersionStatus.Candidate);
        var arquivada = Versao(model.Id, "1.1.1", ModelVersionStatus.Archived);
        var deOutro = Versao(Guid.NewGuid(), "9.9.9", ModelVersionStatus.Approved);

        model.SetActiveVersion(aprovada);
        Assert.Equal(aprovada.Id, model.ActiveVersionId);
        Assert.True(model.IsActiveVersion(aprovada.Id));

        Assert.Throws<DomainRuleException>(() => model.SetActiveVersion(candidata));
        Assert.Throws<DomainRuleException>(() => model.SetActiveVersion(arquivada));
        Assert.Throws<DomainRuleException>(() => model.SetActiveVersion(deOutro));
        Assert.Equal(aprovada.Id, model.ActiveVersionId);
    }

    [Fact]
    public async Task SetActive_VersaoAprovada_GravaOPonteiroEDevolveOModelo()
    {
        var (repository, model) = CatalogoCom();
        var v101 = Versao(model.Id, "1.0.1", ModelVersionStatus.Approved);
        var v111 = Versao(model.Id, "1.1.1", ModelVersionStatus.Approved);
        repository.SeedVersion(v101);
        repository.SeedVersion(v111);

        var useCase = new SetActiveModelVersionUseCase(repository);

        var result = await useCase.ExecuteAsync(model.Id, new SetActiveVersionRequest(v111.Id), CancellationToken.None);

        Assert.True(result.IsSuccess, string.Join("; ", result.Errors.Select(e => e.Message)));
        Assert.Equal(v111.Id, result.Value.ActiveVersionId);
        Assert.Equal(v111.Id, model.ActiveVersionId);
    }

    [Fact]
    public async Task SetActive_VersaoArquivada_RecusaSemMexerNoPonteiro()
    {
        var (repository, model) = CatalogoCom();
        var ativa = Versao(model.Id, "1.0.1", ModelVersionStatus.Approved);
        var arquivada = Versao(model.Id, "1.1.1", ModelVersionStatus.Archived);
        repository.SeedVersion(ativa);
        repository.SeedVersion(arquivada);
        model.SetActiveVersion(ativa);

        var useCase = new SetActiveModelVersionUseCase(repository);

        var result = await useCase.ExecuteAsync(model.Id, new SetActiveVersionRequest(arquivada.Id), CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Contains(result.Errors, e => e.Message == ModelCatalogErrors.ActiveVersionMustBeApproved("Archived"));
        Assert.Equal(ativa.Id, model.ActiveVersionId);
    }

    [Fact]
    public async Task SetActive_JaAtiva_EIdempotente()
    {
        var (repository, model) = CatalogoCom();
        var ativa = Versao(model.Id, "1.0.1", ModelVersionStatus.Approved);
        repository.SeedVersion(ativa);
        model.SetActiveVersion(ativa);

        var useCase = new SetActiveModelVersionUseCase(repository);

        var result = await useCase.ExecuteAsync(model.Id, new SetActiveVersionRequest(ativa.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ativa.Id, result.Value.ActiveVersionId);
    }

    [Fact]
    public async Task Archive_VersaoAtiva_Recusa()
    {
        var (repository, model) = CatalogoCom();
        var ativa = Versao(model.Id, "1.0.1", ModelVersionStatus.Approved);
        repository.SeedVersion(ativa);
        model.SetActiveVersion(ativa);

        var useCase = new ArchiveModelVersionUseCase(repository);

        var result = await useCase.ExecuteAsync(model.Id, ativa.Id, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Contains(result.Errors, e => e.Message == ModelCatalogErrors.CannotArchiveActiveVersion);
        Assert.Equal(ModelVersionStatus.Approved, ativa.Status);
    }

    [Fact]
    public async Task Unarchive_UseCase_VoltaParaApprovedMasNaoViraAtiva()
    {
        var (repository, model) = CatalogoCom();
        var ativa = Versao(model.Id, "1.0.1", ModelVersionStatus.Approved);
        var arquivada = Versao(model.Id, "1.1.1", ModelVersionStatus.Archived);
        repository.SeedVersion(ativa);
        repository.SeedVersion(arquivada);
        model.SetActiveVersion(ativa);

        var useCase = new UnarchiveModelVersionUseCase(repository);

        var result = await useCase.ExecuteAsync(model.Id, arquivada.Id, CancellationToken.None);

        Assert.True(result.IsSuccess, string.Join("; ", result.Errors.Select(e => e.Message)));
        Assert.Equal(ModelVersionStatus.Approved, result.Value.Status);
        Assert.Equal(ativa.Id, model.ActiveVersionId);
    }

    [Fact]
    public async Task Unarchive_UseCase_VersaoNaoArquivada_RecusaComErroDeTransicao()
    {
        var (repository, model) = CatalogoCom();
        var aprovada = Versao(model.Id, "1.0.1", ModelVersionStatus.Approved);
        repository.SeedVersion(aprovada);

        var useCase = new UnarchiveModelVersionUseCase(repository);

        var result = await useCase.ExecuteAsync(model.Id, aprovada.Id, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Contains(result.Errors, e => e.Message == ModelCatalogErrors.InvalidTransition("Archived", "Approved"));
    }

    [Fact]
    public async Task DeleteVersion_ApagaOArquivoEALinha()
    {
        var (repository, model) = CatalogoCom();
        var ativa = Versao(model.Id, "1.0.1", ModelVersionStatus.Approved);
        var velha = Versao(model.Id, "1.0.0", ModelVersionStatus.Archived);
        repository.SeedVersion(ativa);
        repository.SeedVersion(velha);
        model.SetActiveVersion(ativa);

        var storage = new FakeModelArtifactStorage();
        storage.Seed(velha.ArtifactObjectKey!, [1, 2, 3]);

        var useCase = new DeleteModelVersionUseCase(repository, storage);

        var result = await useCase.ExecuteAsync(model.Id, velha.Id, CancellationToken.None);

        Assert.True(result.IsSuccess, string.Join("; ", result.Errors.Select(e => e.Message)));
        Assert.Equal(1, storage.DeleteCalls);
        Assert.Null(storage.Conteudo(velha.ArtifactObjectKey!));
        Assert.DoesNotContain(repository.Versions, v => v.Id == velha.Id);
        Assert.Contains(repository.Versions, v => v.Id == ativa.Id);
    }

    [Fact]
    public async Task DeleteVersion_VersaoAtiva_RecusaSemTocarNoStorage()
    {
        var (repository, model) = CatalogoCom();
        var ativa = Versao(model.Id, "1.0.1", ModelVersionStatus.Approved);
        repository.SeedVersion(ativa);
        model.SetActiveVersion(ativa);

        var storage = new FakeModelArtifactStorage();
        var useCase = new DeleteModelVersionUseCase(repository, storage);

        var result = await useCase.ExecuteAsync(model.Id, ativa.Id, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Contains(result.Errors, e => e.Message == ModelCatalogErrors.CannotDeleteActiveVersion);
        Assert.Equal(0, storage.TotalCalls);
        Assert.Contains(repository.Versions, v => v.Id == ativa.Id);
    }

    [Fact]
    public async Task DeleteVersion_StorageIndisponivel_MantemALinhaEResponde503()
    {
        var (repository, model) = CatalogoCom();
        var velha = Versao(model.Id, "1.0.0", ModelVersionStatus.Archived);
        repository.SeedVersion(velha);

        var storage = new FakeModelArtifactStorage { Falha = FakeModelArtifactStorage.Indisponivel() };
        var useCase = new DeleteModelVersionUseCase(repository, storage);

        var result = await useCase.ExecuteAsync(model.Id, velha.Id, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.True(result.HasError<UpstreamUnavailableError>());
        Assert.Contains(repository.Versions, v => v.Id == velha.Id);
    }

    [Fact]
    public async Task DeleteVersion_Inexistente_DevolveVersionNotFound()
    {
        var (repository, model) = CatalogoCom();
        var useCase = new DeleteModelVersionUseCase(repository, new FakeModelArtifactStorage());

        var result = await useCase.ExecuteAsync(model.Id, Guid.NewGuid(), CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Contains(result.Errors, e => e.Message == ModelCatalogErrors.VersionNotFound);
    }

    [Fact]
    public async Task GetModel_ExpoeAVersaoAtivaNaResposta()
    {
        var (repository, model) = CatalogoCom();
        var aprovada = Versao(model.Id, "1.0.1", ModelVersionStatus.Approved);
        repository.SeedVersion(aprovada);
        model.SetActiveVersion(aprovada);

        var result = await new GetModelUseCase(repository).ExecuteAsync(model.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(aprovada.Id, result.Value.ActiveVersionId);
    }
}
