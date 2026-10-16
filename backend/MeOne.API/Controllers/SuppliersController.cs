using MeOne.API.Auth;
using MeOne.API.Errors;
using MeOne.Application.Communication.Suppliers.Dtos;
using MeOne.Application.Communication.Suppliers.Requests;
using MeOne.Application.Communication.Suppliers.Responses;
using MeOne.Application.Interfaces.Suppliers;
using MeOne.Domain.Errors;
using MeOne.Domain.Rules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace MeOne.API.Controllers;

[ApiController]
[Route("api/v1/suppliers")]
public class SuppliersController(
    IListSuppliersUseCase listSuppliers,
    IGetSupplierUseCase getSupplier,
    ICreateSupplierUseCase createSupplier,
    IUpdateSupplierUseCase updateSupplier,
    IListSupplierUsersUseCase listUsers,
    IAddSupplierToolUseCase addTool,
    IUpdateSupplierToolUseCase updateTool,
    IRemoveSupplierToolUseCase removeTool,
    IAddSupplierMaterialUseCase addMaterial,
    IUpdateSupplierMaterialUseCase updateMaterial,
    IRemoveSupplierMaterialUseCase removeMaterial,
    IAddSupplierSelfAssessmentUseCase addSelfAssessment,
    IUpdateSupplierSelfAssessmentUseCase updateSelfAssessment,
    IRemoveSupplierSelfAssessmentUseCase removeSelfAssessment,
    IAddSupplierAuditUseCase addAudit,
    IUpdateSupplierAuditUseCase updateAudit,
    IRemoveSupplierAuditUseCase removeAudit,
    IAddSupplierDocumentUseCase addDocument,
    IRemoveSupplierDocumentUseCase removeDocument,
    IGetSupplierDocumentUseCase getDocument) : ControllerBase
{
    private const long MaxRequestBytes = SupplierErrors.MaxFileBytes + (1024 * 1024);

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(SupplierPageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SupplierPageResponse>> List([FromQuery] SuppliersQuery query, CancellationToken cancellationToken)
    {
        var result = await listSuppliers.ExecuteAsync(query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Erro ao buscar fornecedores", result);
    }

    [HttpGet("users")]
    [RequirePermission(Permissions.SuppliersRegister)]
    [ProducesResponseType(typeof(IReadOnlyList<SupplierUserResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SupplierUserResponse>>> Users(CancellationToken cancellationToken)
    {
        var result = await listUsers.ExecuteAsync(cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Erro ao buscar usuários", result);
    }

    [HttpGet("{id:long}")]
    [Authorize]
    [ProducesResponseType(typeof(SupplierDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SupplierDetailResponse>> GetById(ulong id, CancellationToken cancellationToken)
    {
        var result = await getSupplier.ExecuteAsync(id, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Falha ao buscar fornecedor", result);
    }

    [HttpPost]
    [RequirePermission(Permissions.SuppliersRegister)]
    [ProducesResponseType(typeof(SupplierSummaryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SupplierSummaryResponse>> Create(
        [FromBody] SaveSupplierRequest request,
        CancellationToken cancellationToken)
    {
        var result = await createSupplier.ExecuteAsync(request, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : ApiResults.Failure("Falha ao cadastrar fornecedor", result);
    }

    [HttpPut("{id:long}")]
    [RequirePermission(Permissions.SuppliersRegister)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(ulong id, [FromBody] SaveSupplierRequest request, CancellationToken cancellationToken)
    {
        var result = await updateSupplier.ExecuteAsync(id, request, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao atualizar fornecedor", result);
    }

    [HttpPost("{id:long}/tools")]
    [RequirePermission(Permissions.SuppliersRegister)]
    [ProducesResponseType(typeof(SupplierToolResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SupplierToolResponse>> AddTool(
        ulong id,
        [FromBody] SaveSupplierToolRequest request,
        CancellationToken cancellationToken)
    {
        var result = await addTool.ExecuteAsync(id, request, cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : ApiResults.Failure("Falha ao adicionar ferramental", result);
    }

    [HttpPut("{id:long}/tools/{itemId:long}")]
    [RequirePermission(Permissions.SuppliersRegister)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTool(
        ulong id,
        ulong itemId,
        [FromBody] SaveSupplierToolRequest request,
        CancellationToken cancellationToken)
    {
        var result = await updateTool.ExecuteAsync(id, itemId, request, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao atualizar ferramental", result);
    }

    [HttpDelete("{id:long}/tools/{itemId:long}")]
    [RequirePermission(Permissions.SuppliersRegister)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveTool(ulong id, ulong itemId, CancellationToken cancellationToken)
    {
        var result = await removeTool.ExecuteAsync(id, itemId, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao remover ferramental", result);
    }

    [HttpPost("{id:long}/materials")]
    [RequirePermission(Permissions.SuppliersRegister)]
    [ProducesResponseType(typeof(SupplierMaterialResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SupplierMaterialResponse>> AddMaterial(
        ulong id,
        [FromBody] SaveSupplierMaterialRequest request,
        CancellationToken cancellationToken)
    {
        var result = await addMaterial.ExecuteAsync(id, request, cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : ApiResults.Failure("Falha ao adicionar material", result);
    }

    [HttpPut("{id:long}/materials/{itemId:long}")]
    [RequirePermission(Permissions.SuppliersRegister)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMaterial(
        ulong id,
        ulong itemId,
        [FromBody] SaveSupplierMaterialRequest request,
        CancellationToken cancellationToken)
    {
        var result = await updateMaterial.ExecuteAsync(id, itemId, request, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao atualizar material", result);
    }

    [HttpDelete("{id:long}/materials/{itemId:long}")]
    [RequirePermission(Permissions.SuppliersRegister)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMaterial(ulong id, ulong itemId, CancellationToken cancellationToken)
    {
        var result = await removeMaterial.ExecuteAsync(id, itemId, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao remover material", result);
    }

    [HttpPost("{id:long}/self-assessments")]
    [RequirePermission(Permissions.SuppliersRegister)]
    [ProducesResponseType(typeof(SupplierSelfAssessmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SupplierSelfAssessmentResponse>> AddSelfAssessment(
        ulong id,
        [FromBody] SaveSelfAssessmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await addSelfAssessment.ExecuteAsync(id, request, cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : ApiResults.Failure("Falha ao adicionar auto avaliação", result);
    }

    [HttpPut("{id:long}/self-assessments/{itemId:long}")]
    [RequirePermission(Permissions.SuppliersRegister)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSelfAssessment(
        ulong id,
        ulong itemId,
        [FromBody] SaveSelfAssessmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await updateSelfAssessment.ExecuteAsync(id, itemId, request, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao atualizar auto avaliação", result);
    }

    [HttpDelete("{id:long}/self-assessments/{itemId:long}")]
    [RequirePermission(Permissions.SuppliersRegister)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveSelfAssessment(ulong id, ulong itemId, CancellationToken cancellationToken)
    {
        var result = await removeSelfAssessment.ExecuteAsync(id, itemId, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao remover auto avaliação", result);
    }

    [HttpPost("{id:long}/audits")]
    [RequirePermission(Permissions.SuppliersRegister)]
    [ProducesResponseType(typeof(SupplierAuditResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SupplierAuditResponse>> AddAudit(
        ulong id,
        [FromBody] SaveAuditRequest request,
        CancellationToken cancellationToken)
    {
        var result = await addAudit.ExecuteAsync(id, request, cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : ApiResults.Failure("Falha ao adicionar auditoria", result);
    }

    [HttpPut("{id:long}/audits/{itemId:long}")]
    [RequirePermission(Permissions.SuppliersRegister)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAudit(
        ulong id,
        ulong itemId,
        [FromBody] SaveAuditRequest request,
        CancellationToken cancellationToken)
    {
        var result = await updateAudit.ExecuteAsync(id, itemId, request, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao atualizar auditoria", result);
    }

    [HttpDelete("{id:long}/audits/{itemId:long}")]
    [RequirePermission(Permissions.SuppliersRegister)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveAudit(ulong id, ulong itemId, CancellationToken cancellationToken)
    {
        var result = await removeAudit.ExecuteAsync(id, itemId, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao remover auditoria", result);
    }

    [HttpPost("{id:long}/documents")]
    [RequirePermission(Permissions.SuppliersRegister)]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxRequestBytes)]
    [ProducesResponseType(typeof(SupplierDocumentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SupplierDocumentResponse>> AddDocument(
        ulong id,
        [FromForm] AddSupplierDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var file = Request.Form.Files.GetFile("file");
        var upload = file is null ? null : new SupplierFileUpload(Path.GetFileName(file.FileName), file.Length, file.OpenReadStream());

        var result = await addDocument.ExecuteAsync(id, request, upload, cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : ApiResults.Failure("Falha ao anexar documento", result);
    }

    [HttpDelete("{id:long}/documents/{documentId:long}")]
    [RequirePermission(Permissions.SuppliersRegister)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveDocument(ulong id, ulong documentId, CancellationToken cancellationToken)
    {
        var result = await removeDocument.ExecuteAsync(id, documentId, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao remover documento", result);
    }

    [HttpGet("{id:long}/documents/{documentId:long}/file")]
    [Authorize]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DocumentFile(ulong id, ulong documentId, CancellationToken cancellationToken)
    {
        var result = await getDocument.ExecuteAsync(id, documentId, cancellationToken);

        if (result.IsFailed)
            return ApiResults.Failure("Falha ao abrir o documento", result);

        Response.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline")
        {
            FileNameStar = result.Value.FileName,
        }.ToString();

        return File(result.Value.Content, result.Value.ContentType);
    }
}
