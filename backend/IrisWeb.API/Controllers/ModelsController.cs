using IrisWeb.API.Auth;
using IrisWeb.API.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IrisWeb.Application.Communication.ModelCatalog.Responses;
using IrisWeb.Application.Communication.ModelCatalog.Requests;
using IrisWeb.Application.Interfaces.ModelCatalog;
using IrisWeb.Domain.Errors;

namespace IrisWeb.API.Controllers;

[ApiController]
[Route("api/v1/models")]
public class ModelsController(
    IListModelsUseCase listModels,
    IListEmbeddingSpacesUseCase listEmbeddingSpaces,
    IGetModelUseCase getModel,
    ICreateModelUseCase createModel,
    IDeleteModelUseCase deleteModel,
    ICreateModelVersionUseCase createVersion,
    ICreateUploadUrlUseCase createUploadUrl,
    IUploadModelArtifactUseCase uploadArtifact,
    ICompleteUploadUseCase completeUpload,
    IApproveModelVersionUseCase approveVersion,
    IArchiveModelVersionUseCase archiveVersion,
    IUnarchiveModelVersionUseCase unarchiveVersion,
    IDeleteModelVersionUseCase deleteVersion,
    ISetActiveModelVersionUseCase setActiveVersion,
    ICreateDownloadUrlUseCase createDownloadUrl,
    IDownloadModelArtifactUseCase downloadArtifact) : ControllerBase
{

    private const string ArtifactContentType = "application/zip";

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<CatalogModelResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CatalogModelResponse>>> List(CancellationToken cancellationToken)
    {
        var result = await listModels.ExecuteAsync(cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : ApiResults.Failure("Não foi possível listar os modelos", result);
    }

    [HttpGet("embedding-spaces")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<EmbeddingSpaceResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EmbeddingSpaceResponse>>> ListEmbeddingSpaces(
        CancellationToken cancellationToken)
    {
        var result = await listEmbeddingSpaces.ExecuteAsync(cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : ApiResults.Failure("Não foi possível listar os espaços de embedding", result);
    }

    [HttpGet("{modelId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CatalogModelResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CatalogModelResponse>> Get(Guid modelId, CancellationToken cancellationToken)
    {
        var result = await getModel.ExecuteAsync(modelId, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);

        return ApiResults.Failure("Erro ao buscar o modelo", result);
    }

    [HttpGet("{modelId:guid}/versions/{versionId:guid}/download-url")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(DownloadUrlResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<DownloadUrlResponse>> CreateDownloadUrl(
        Guid modelId,
        Guid versionId,
        CancellationToken cancellationToken)
    {
        var result = await createDownloadUrl.ExecuteAsync(modelId, versionId, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);

        return ApiResults.Failure("Erro ao fazer o download do modelo", result);
    }

    [HttpGet("{modelId:guid}/versions/{versionId:guid}/artifact")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult> DownloadArtifact(
        Guid modelId,
        Guid versionId,
        CancellationToken cancellationToken)
    {
        var result = await downloadArtifact.ExecuteAsync(modelId, versionId, cancellationToken);

        if (!result.IsSuccess)
            return ApiResults.Failure("Erro ao fazer o download do modelo", result);

        return File(result.Value, ArtifactContentType);
    }

    [HttpPut("{modelId:guid}/versions/{versionId:guid}/artifact")]
    [Authorize]
    [DisableRequestSizeLimit]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult> UploadArtifact(
        Guid modelId,
        Guid versionId,
        CancellationToken cancellationToken)
    {
        var result = await uploadArtifact.ExecuteAsync(modelId, versionId, Request.Body, cancellationToken);

        if (result.IsSuccess)
            return NoContent();

        return ApiResults.Failure("Não foi possível enviar o artefato", result);
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(CatalogModelResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CatalogModelResponse>> Create(
        CreateModelRequest request,
        CancellationToken cancellationToken)
    {
        var result = await createModel.ExecuteAsync(request, cancellationToken);

        if (!result.IsSuccess)
            return ApiResults.Failure("Erro ao criar o modelo", result);

        return CreatedAtAction(nameof(Get), new { modelId = result.Value.Id }, result.Value);
    }

    [HttpDelete("{modelId:guid}")]
    [Authorize(Policy = AuthPolicies.MasterOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult> Delete(Guid modelId, CancellationToken cancellationToken)
    {
        var result = await deleteModel.ExecuteAsync(modelId, cancellationToken);

        if (result.IsSuccess)
            return NoContent();

        if (result.Errors.Any(error => error.Message == ModelCatalogErrors.ModelNotFound))
            return NotFound(ErrorResponse.From("Modelo não encontrado", ModelCatalogErrors.ModelNotFound));

        return ApiResults.Failure("Não foi possível excluir o modelo", result);
    }

    [HttpPost("{modelId:guid}/versions")]
    [Authorize]
    [ProducesResponseType(typeof(CatalogModelVersionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CatalogModelVersionResponse>> CreateVersion(
        Guid modelId,
        CreateModelVersionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await createVersion.ExecuteAsync(modelId, request, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);

        return ApiResults.Failure("Erro ao versionar o modelo", result);
    }

    [HttpPost("{modelId:guid}/versions/{versionId:guid}/upload-url")]
    [Authorize]
    [ProducesResponseType(typeof(UploadUrlResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<UploadUrlResponse>> CreateUploadUrl(
        Guid modelId,
        Guid versionId,
        CancellationToken cancellationToken)
    {
        var result = await createUploadUrl.ExecuteAsync(modelId, versionId, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);

        return ApiResults.Failure("Não foi possível iniciar o upload", result);
    }

    [HttpPost("{modelId:guid}/versions/{versionId:guid}/complete")]
    [Authorize]
    [ProducesResponseType(typeof(CatalogModelVersionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<CatalogModelVersionResponse>> CompleteUpload(
        Guid modelId,
        Guid versionId,
        CompleteUploadRequest request,
        CancellationToken cancellationToken)
    {
        var result = await completeUpload.ExecuteAsync(modelId, versionId, request, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);

        return ApiResults.Failure("Não foi possível concluir o upload", result);
    }

    [HttpPost("{modelId:guid}/versions/{versionId:guid}/approve")]
    [Authorize(Policy = AuthPolicies.MasterOnly)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(CatalogModelVersionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CatalogModelVersionResponse>> Approve(
        Guid modelId,
        Guid versionId,
        CancellationToken cancellationToken)
    {
        var result = await approveVersion.ExecuteAsync(modelId, versionId, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);

        return ApiResults.Failure("Não foi possível aprovar a versão", result);
    }

    [HttpPost("{modelId:guid}/versions/{versionId:guid}/archive")]
    [Authorize(Policy = AuthPolicies.MasterOnly)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> Archive(
        Guid modelId,
        Guid versionId,
        CancellationToken cancellationToken)
    {
        var result = await archiveVersion.ExecuteAsync(modelId, versionId, cancellationToken);

        if (result.IsSuccess)
            return NoContent();

        return ApiResults.Failure("Não foi possível arquivar o modelo", result);
    }

    [HttpPost("{modelId:guid}/versions/{versionId:guid}/unarchive")]
    [Authorize(Policy = AuthPolicies.MasterOnly)]
    [ProducesResponseType(typeof(CatalogModelVersionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CatalogModelVersionResponse>> Unarchive(
        Guid modelId,
        Guid versionId,
        CancellationToken cancellationToken)
    {
        var result = await unarchiveVersion.ExecuteAsync(modelId, versionId, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);

        return ApiResults.Failure("Não foi possível desarquivar a versão", result);
    }

    [HttpDelete("{modelId:guid}/versions/{versionId:guid}")]
    [Authorize(Policy = AuthPolicies.MasterOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult> DeleteVersion(
        Guid modelId,
        Guid versionId,
        CancellationToken cancellationToken)
    {
        var result = await deleteVersion.ExecuteAsync(modelId, versionId, cancellationToken);

        if (result.IsSuccess)
            return NoContent();

        if (result.Errors.Any(error =>
                error.Message == ModelCatalogErrors.ModelNotFound ||
                error.Message == ModelCatalogErrors.VersionNotFound))
        {
            return NotFound(ErrorResponse.From("Versão não encontrada", result.Errors.Select(error => error.Message)));
        }

        return ApiResults.Failure("Não foi possível excluir a versão", result);
    }

    [HttpPut("{modelId:guid}/active-version")]
    [Authorize(Policy = AuthPolicies.MasterOnly)]
    [ProducesResponseType(typeof(CatalogModelResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CatalogModelResponse>> SetActiveVersion(
        Guid modelId,
        SetActiveVersionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await setActiveVersion.ExecuteAsync(modelId, request, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);

        return ApiResults.Failure("Não foi possível ativar a versão", result);
    }
}
