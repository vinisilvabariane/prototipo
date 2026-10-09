using System.Text.Json;
using IrisWeb.Application.Communication.ModelCatalog.Requests;
using IrisWeb.Application.Errors;
using IrisWeb.Domain.Entities;
using IrisWeb.Domain.Enums;
using IrisWeb.Domain.Errors;
using IrisWeb.Tests.Fakes;
using IrisWeb.Application.Interfaces.ModelCatalog;
using IrisWeb.Application.UseCases.ModelCatalog;

namespace IrisWeb.Tests.Application;

public sealed class ModelCatalogUseCaseTests
{
    private static JsonElement Manifest() =>
        JsonSerializer.Deserialize<JsonElement>("""{"entrypoint":"model.onnx"}""");

    private static (FakeModelCatalogRepository Repository, AiModel Model) CatalogoCom(AiModel? modelo = null)
    {
        var repository = new FakeModelCatalogRepository();
        var model = modelo ?? AiModel.Create("facenet", "FaceNet", null, AiTaskType.FaceRecognition, ModelPlatform.Web, "vinicius");
        repository.Seed(model);
        return (repository, model);
    }

    private static ModelVersion VersaoEmUpload(Guid modelId)
    {
        var version = ModelVersion.Create(modelId, "1.0.0", "onnxruntime-web", "vinicius");
        version.BeginUpload("facenet/1.0.0/model-package.zip");
        return version;
    }

    private static ModelVersion VersaoCandidata(Guid modelId)
    {
        var version = VersaoEmUpload(modelId);
        version.MarkCandidate(1024, "ABC123", Manifest().GetRawText());
        return version;
    }

    private static ModelVersion VersaoAprovada(Guid modelId)
    {
        var version = VersaoCandidata(modelId);
        version.Approve("vinicius");
        return version;
    }

    [Fact]
    public async Task CreateDownloadUrl_VersaoNaoAprovada_Recusa()
    {
        var (repository, model) = CatalogoCom();
        var version = VersaoCandidata(model.Id);
        repository.SeedVersion(version);

        var useCase = new CreateDownloadUrlUseCase(repository);

        var result = await useCase.ExecuteAsync(model.Id, version.Id, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal(ModelCatalogErrors.NotApproved, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task DownloadArtifact_VersaoNaoAprovada_RecusaSemAbrirOArtefato()
    {
        var (repository, model) = CatalogoCom();
        var version = VersaoCandidata(model.Id);
        repository.SeedVersion(version);

        var storage = new FakeModelArtifactStorage();
        storage.Seed(version.ArtifactObjectKey!, [1, 2, 3]);

        var useCase = new DownloadModelArtifactUseCase(repository, storage);

        var result = await useCase.ExecuteAsync(model.Id, version.Id, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal(ModelCatalogErrors.NotApproved, Assert.Single(result.Errors).Message);
        Assert.Equal(0, storage.TotalCalls);
    }

    [Fact]
    public async Task DownloadArtifact_VersaoAprovada_DevolveOConteudoGravado()
    {
        var (repository, model) = CatalogoCom();
        var version = VersaoAprovada(model.Id);
        repository.SeedVersion(version);

        var storage = new FakeModelArtifactStorage();
        storage.Seed(version.ArtifactObjectKey!, [7, 7, 7, 7]);

        var useCase = new DownloadModelArtifactUseCase(repository, storage);

        var result = await useCase.ExecuteAsync(model.Id, version.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);

        using var buffer = new MemoryStream();
        await result.Value.CopyToAsync(buffer, CancellationToken.None);
        Assert.Equal<byte[]>([7, 7, 7, 7], buffer.ToArray());
    }

    [Fact]
    public async Task DownloadArtifact_ArtefatoForaDoStorage_RecusaComMensagemPropria()
    {
        var (repository, model) = CatalogoCom();
        var version = VersaoAprovada(model.Id);
        repository.SeedVersion(version);

        var useCase = new DownloadModelArtifactUseCase(repository, new FakeModelArtifactStorage());

        var result = await useCase.ExecuteAsync(model.Id, version.Id, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal(ModelCatalogErrors.ArtifactNotInStorage, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task CreateDownloadUrl_VersaoAprovada_DevolveUrlRelativaDaPropriaApi()
    {
        var (repository, model) = CatalogoCom();
        var version = VersaoAprovada(model.Id);
        repository.SeedVersion(version);

        var useCase = new CreateDownloadUrlUseCase(repository);

        var result = await useCase.ExecuteAsync(model.Id, version.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal($"/api/v1/models/{model.Id}/versions/{version.Id}/artifact", result.Value.Url);
    }

    [Fact]
    public async Task UploadArtifact_StorageForaDoAr_FalhaComErroDeIndisponibilidade()
    {
        var (repository, model) = CatalogoCom();
        var version = VersaoEmUpload(model.Id);
        repository.SeedVersion(version);

        var storage = new FakeModelArtifactStorage
        {
            Falha = FakeModelArtifactStorage.Indisponivel(),
        };

        var useCase = new UploadModelArtifactUseCase(repository, storage);

        var result = await useCase.ExecuteAsync(
            model.Id, version.Id, new MemoryStream([1, 2, 3]), CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.True(
            result.HasError<UpstreamUnavailableError>(),
            "Sem o erro tipado o ApiResults responde 400 e o cliente trata disco fora do ar como pedido invalido.");
    }

    [Fact]
    public async Task UploadArtifact_VersaoEmUpload_GravaOsBytesNaChaveDaVersao()
    {
        var (repository, model) = CatalogoCom();
        var version = VersaoEmUpload(model.Id);
        repository.SeedVersion(version);

        var storage = new FakeModelArtifactStorage();
        var useCase = new UploadModelArtifactUseCase(repository, storage);

        var result = await useCase.ExecuteAsync(
            model.Id, version.Id, new MemoryStream([9, 8, 7]), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal<byte[]>([9, 8, 7], storage.Conteudo(version.ArtifactObjectKey!)!);
    }

    [Fact]
    public async Task UploadArtifact_VersaoAprovada_RecusaSemGravar()
    {
        var (repository, model) = CatalogoCom();
        var version = VersaoAprovada(model.Id);
        repository.SeedVersion(version);

        var storage = new FakeModelArtifactStorage();
        var useCase = new UploadModelArtifactUseCase(repository, storage);

        var result = await useCase.ExecuteAsync(
            model.Id, version.Id, new MemoryStream([1]), CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal(
            ModelCatalogErrors.InvalidTransition(
                nameof(ModelVersionStatus.Uploading),
                nameof(ModelVersionStatus.Approved)),
            Assert.Single(result.Errors).Message);
        Assert.Equal(0, storage.TotalCalls);
    }

    [Fact]
    public async Task CompleteUpload_TamanhoDivergente_RecusaEMantemAVersaoEmUploading()
    {
        var (repository, model) = CatalogoCom();
        var version = VersaoEmUpload(model.Id);
        repository.SeedVersion(version);

        var storage = new FakeModelArtifactStorage
        {
            Metadata = new StoredObjectMetadata(2048, "etag-do-storage"),
        };

        var useCase = new CompleteUploadUseCase(repository, storage);

        var result = await useCase.ExecuteAsync(
            model.Id,
            version.Id,
            new CompleteUploadRequest(1024, "abc123", Manifest()),
            CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal(ModelCatalogErrors.ArtifactSizeMismatch, Assert.Single(result.Errors).Message);
        Assert.Equal(ModelVersionStatus.Uploading, version.Status);
        Assert.Null(version.ArtifactSize);
    }

    [Fact]
    public async Task CreateModel_TokenSemMatricula_RecusaSemGravar()
    {
        var repository = new FakeModelCatalogRepository();

        var useCase = new CreateModelUseCase(repository, new FakeCurrentActor(null));

        var result = await useCase.ExecuteAsync(
            new CreateModelRequest("facenet", "FaceNet", null, AiTaskType.FaceRecognition, ModelPlatform.Web),
            CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal(ModelCatalogErrors.ActorUnknown, Assert.Single(result.Errors).Message);
        Assert.Empty(repository.Stored);
    }

    [Fact]
    public async Task ArchiveModelVersion_VersaoJaArquivada_ContinuaRespondendoSucesso()
    {
        var (repository, model) = CatalogoCom();
        var version = VersaoCandidata(model.Id);
        repository.SeedVersion(version);

        var useCase = new ArchiveModelVersionUseCase(repository);

        var primeiro = await useCase.ExecuteAsync(model.Id, version.Id, CancellationToken.None);
        var segundo = await useCase.ExecuteAsync(model.Id, version.Id, CancellationToken.None);

        Assert.True(primeiro.IsSuccess);
        Assert.True(
            segundo.IsSuccess,
            "Rearquivar e idempotente: o rowcount zero e a ausencia de mudanca, nao uma falha.");
        Assert.Equal(ModelVersionStatus.Archived, version.Status);
    }

    [Fact]
    public async Task ApproveModelVersion_VersaoEmUpload_RecusaComOErroDeTransicao()
    {
        var (repository, model) = CatalogoCom();
        var version = VersaoEmUpload(model.Id);
        repository.SeedVersion(version);

        var useCase = new ApproveModelVersionUseCase(repository, new FakeCurrentActor());

        var result = await useCase.ExecuteAsync(model.Id, version.Id, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal(
            ModelCatalogErrors.InvalidTransition(
                nameof(ModelVersionStatus.Candidate),
                nameof(ModelVersionStatus.Uploading)),
            Assert.Single(result.Errors).Message);
        Assert.Equal(ModelVersionStatus.Uploading, version.Status);
    }

    [Fact]
    public async Task ListModels_ManifestInvalido_DegradaParaSemManifestSemDerrubarALista()
    {
        var (repository, model) = CatalogoCom();
        var version = VersaoEmUpload(model.Id);
        version.MarkCandidate(1024, "abc123", "isto nao e json");
        repository.SeedVersion(version);
        model.Versions.Add(version);

        var useCase = new ListModelsUseCase(repository);

        var result = await useCase.ExecuteAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(Assert.Single(Assert.Single(result.Value).Versions).Manifest);
    }

    [Fact]
    public async Task CreateModel_SaveSemLinhaAfetada_FalhaSemDevolverId()
    {
        var repository = new FakeModelCatalogRepository { NadaPersiste = true };

        var useCase = new CreateModelUseCase(repository, new FakeCurrentActor());

        var result = await useCase.ExecuteAsync(
            new CreateModelRequest("facenet", "FaceNet", null, AiTaskType.FaceRecognition, ModelPlatform.Web),
            CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal(ModelCatalogErrors.NothingPersisted, Assert.Single(result.Errors).Message);
        Assert.Null(result.ValueOrDefault);
    }

    [Fact]
    public async Task CreateUploadUrl_SaveSemLinhaAfetada_Falha()
    {
        var (repository, model) = CatalogoCom();
        var version = ModelVersion.Create(model.Id, "1.0.0", "onnxruntime-web", "vinicius");
        repository.SeedVersion(version);
        repository.NadaPersiste = true;

        var useCase = new CreateUploadUrlUseCase(repository);

        var result = await useCase.ExecuteAsync(model.Id, version.Id, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal(ModelCatalogErrors.NothingPersisted, Assert.Single(result.Errors).Message);
        Assert.Null(result.ValueOrDefault);
    }

    [Fact]
    public async Task CreateUploadUrl_VersaoEmDraft_DevolveUrlRelativaDaPropriaApi()
    {
        var (repository, model) = CatalogoCom();
        var version = ModelVersion.Create(model.Id, "1.0.0", "onnxruntime-web", "vinicius");
        repository.SeedVersion(version);

        var useCase = new CreateUploadUrlUseCase(repository);

        var result = await useCase.ExecuteAsync(model.Id, version.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("PUT", result.Value.Method);
        Assert.Equal($"/api/v1/models/{model.Id}/versions/{version.Id}/artifact", result.Value.Url);
        Assert.Equal(ModelVersionStatus.Uploading, version.Status);
    }

    [Fact]
    public async Task ApproveModelVersion_TokenSemMatricula_RecusaEMantemAVersaoNaoAprovada()
    {
        var (repository, model) = CatalogoCom();
        var version = VersaoCandidata(model.Id);
        repository.SeedVersion(version);

        var useCase = new ApproveModelVersionUseCase(repository, new FakeCurrentActor(null));

        var result = await useCase.ExecuteAsync(model.Id, version.Id, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal(ModelCatalogErrors.ActorUnknown, Assert.Single(result.Errors).Message);
        Assert.Equal(ModelVersionStatus.Candidate, version.Status);
        Assert.Null(version.ApprovedBy);
    }

    [Fact]
    public async Task ApproveModelVersion_ComMatricula_AprovaEGravaAAutoria()
    {
        var (repository, model) = CatalogoCom();
        var version = VersaoCandidata(model.Id);
        repository.SeedVersion(version);

        var useCase = new ApproveModelVersionUseCase(repository, new FakeCurrentActor("123456"));

        var result = await useCase.ExecuteAsync(model.Id, version.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ModelVersionStatus.Approved, version.Status);
        Assert.Equal("123456", version.ApprovedBy);
    }

    [Fact]
    public async Task CreateModelVersion_RuntimeDeOutraPlataforma_RecusaSemGravar()
    {
        var (repository, model) = CatalogoCom();

        var useCase = new CreateModelVersionUseCase(repository, new FakeCurrentActor());

        var result = await useCase.ExecuteAsync(
            model.Id, new CreateModelVersionRequest("1.0.0", "openvino"), CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal(
            ModelCatalogErrors.RuntimeMismatch("Web", "onnxruntime-web", "openvino"),
            Assert.Single(result.Errors).Message);
        Assert.Empty(repository.Versions);
    }

    [Fact]
    public async Task CreateModelVersion_RuntimeDaPlataforma_CriaIgnorandoCaixa()
    {
        var desktop = AiModel.Create("fpc", "FPC", null, AiTaskType.ObjectDetection, ModelPlatform.Desktop, "vinicius");
        var (repository, model) = CatalogoCom(desktop);

        var useCase = new CreateModelVersionUseCase(repository, new FakeCurrentActor());

        var result = await useCase.ExecuteAsync(
            model.Id, new CreateModelVersionRequest("1.0.0", "OpenVINO"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("openvino", Assert.Single(repository.Versions).Runtime);
    }

    [Fact]
    public async Task CreateModelVersion_RuntimeDeWeb_CriaComOValorGravadoNoCatalogo()
    {
        var (repository, model) = CatalogoCom();

        var useCase = new CreateModelVersionUseCase(repository, new FakeCurrentActor());

        var result = await useCase.ExecuteAsync(
            model.Id, new CreateModelVersionRequest("2.0.0", "onnxruntime-web"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("onnxruntime-web", Assert.Single(repository.Versions).Runtime);
    }

    [Theory]
    [InlineData("onnx")]
    [InlineData("onnxruntime")]
    public async Task CreateModelVersion_SinonimoDoRuntimeDeWeb_RecusaSemGravar(string runtime)
    {
        var (repository, model) = CatalogoCom();

        var useCase = new CreateModelVersionUseCase(repository, new FakeCurrentActor());

        var result = await useCase.ExecuteAsync(
            model.Id, new CreateModelVersionRequest("2.0.0", runtime), CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal(
            ModelCatalogErrors.RuntimeMismatch("Web", "onnxruntime-web", runtime),
            Assert.Single(result.Errors).Message);
        Assert.Empty(repository.Versions);
    }

    [Fact]
    public async Task CompleteUpload_EntrypointDeOutraPlataforma_RecusaEMantemAVersaoEmUploading()
    {
        var (repository, model) = CatalogoCom();
        var version = VersaoEmUpload(model.Id);
        repository.SeedVersion(version);

        var storage = new FakeModelArtifactStorage
        {
            Metadata = new StoredObjectMetadata(1024, "etag"),
        };

        var useCase = new CompleteUploadUseCase(repository, storage);

        var manifest = JsonSerializer.Deserialize<JsonElement>("""{"entrypoint":"model.xml","format":"onnx"}""");

        var result = await useCase.ExecuteAsync(
            model.Id, version.Id, new CompleteUploadRequest(1024, "abc123", manifest), CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal(
            ModelCatalogErrors.EntrypointMismatch("Web", ".onnx", "model.xml"),
            Assert.Single(result.Errors).Message);
        Assert.Equal(ModelVersionStatus.Uploading, version.Status);
        Assert.Equal(0, storage.TotalCalls);
    }

    [Fact]
    public async Task CompleteUpload_FormatoDeOutraPlataforma_Recusa()
    {
        var desktop = AiModel.Create("fpc", "FPC", null, AiTaskType.ObjectDetection, ModelPlatform.Desktop, "vinicius");
        var (repository, model) = CatalogoCom(desktop);
        var version = ModelVersion.Create(model.Id, "1.0.0", "openvino", "vinicius");
        version.BeginUpload("fpc/1.0.0/model-package.zip");
        repository.SeedVersion(version);

        var useCase = new CompleteUploadUseCase(repository, new FakeModelArtifactStorage
        {
            Metadata = new StoredObjectMetadata(1024, "etag"),
        });

        var manifest = JsonSerializer.Deserialize<JsonElement>("""{"entrypoint":"model.xml","format":"onnx"}""");

        var result = await useCase.ExecuteAsync(
            model.Id, version.Id, new CompleteUploadRequest(1024, "abc123", manifest), CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal(
            ModelCatalogErrors.ManifestFormatMismatch("Desktop", "ir", "onnx"),
            Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task CompleteUpload_ManifestoSemFormatoNemEntrypoint_NaoBarraPelaPlataforma()
    {
        var (repository, model) = CatalogoCom();
        var version = VersaoEmUpload(model.Id);
        repository.SeedVersion(version);

        var useCase = new CompleteUploadUseCase(repository, new FakeModelArtifactStorage
        {
            Metadata = new StoredObjectMetadata(1024, "etag"),
        });

        var manifest = JsonSerializer.Deserialize<JsonElement>("""{"embeddingSpace":"x"}""");

        var result = await useCase.ExecuteAsync(
            model.Id, version.Id, new CompleteUploadRequest(1024, new string('a', 64), manifest), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ModelVersionStatus.Candidate, version.Status);
    }

    private static ModelVersion VersaoAprovadaComEspaco(Guid modelId, string embeddingSpace)
    {
        var version = VersaoEmUpload(modelId);
        var manifest = JsonSerializer.Serialize(new { embeddingSpace });
        version.MarkCandidate(1024, "ABC123", manifest);
        version.Approve("vinicius");
        return version;
    }

    [Fact]
    public async Task ListEmbeddingSpaces_AgregaContagemECruzaComVersaoQueDeclaraOMesmoEspaco()
    {
        var (repository, model) = CatalogoCom();
        var version = VersaoAprovadaComEspaco(model.Id, "mobilefacenet_v1");
        repository.SeedVersion(version);
        model.Versions.Add(version);

        var embeddings = new FakeFaceEmbeddingRepository();
        embeddings.Seed("111", [0.1f, 0.2f], "mobilefacenet_v1");
        embeddings.Seed("222", [0.3f, 0.4f], "mobilefacenet_v1");
        embeddings.Seed("333", [0.5f, 0.6f], "inception_resnet_v1_aligned");

        var useCase = new ListEmbeddingSpacesUseCase(embeddings, repository);

        var result = await useCase.ExecuteAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);

        var mobilefacenet = Assert.Single(result.Value, space => space.EmbeddingSpace == "mobilefacenet_v1");
        Assert.Equal(2, mobilefacenet.EnrollmentCount);
        var match = Assert.Single(mobilefacenet.CatalogMatches);
        Assert.Equal(model.Key, match.ModelKey);
        Assert.Equal(version.Version, match.Version);

        var legacy = Assert.Single(result.Value, space => space.EmbeddingSpace == "inception_resnet_v1_aligned");
        Assert.Equal(1, legacy.EnrollmentCount);
        Assert.Empty(legacy.CatalogMatches);
    }

    [Fact]
    public async Task ListEmbeddingSpaces_VersaoSemEmbeddingSpaceNoManifesto_NaoAparecemComoCorrespondencia()
    {
        var (repository, model) = CatalogoCom();
        var version = VersaoAprovada(model.Id);
        repository.SeedVersion(version);
        model.Versions.Add(version);

        var embeddings = new FakeFaceEmbeddingRepository();
        embeddings.Seed("111", [0.1f, 0.2f], "mobilefacenet_v1");

        var useCase = new ListEmbeddingSpacesUseCase(embeddings, repository);

        var result = await useCase.ExecuteAsync(CancellationToken.None);

        var space = Assert.Single(result.Value);
        Assert.Equal(1, space.EnrollmentCount);
        Assert.Empty(space.CatalogMatches);
    }
}
