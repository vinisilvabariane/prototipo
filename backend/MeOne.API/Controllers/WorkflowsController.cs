using MeOne.API.Auth;
using MeOne.API.Errors;
using MeOne.Application.Communication.Common.Responses;
using MeOne.Application.Communication.Workflows.Dtos;
using MeOne.Application.Communication.Workflows.Requests;
using MeOne.Application.Communication.Workflows.Responses;
using MeOne.Application.Interfaces.Workflows;
using MeOne.Domain.Errors;
using MeOne.Domain.Rules;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace MeOne.API.Controllers;

[ApiController]
[Route("api/v1/workflows")]
public class WorkflowsController(
    IListWorkflowsUseCase listWorkflows,
    IGetWorkflowStatsUseCase getStats,
    IListWorkflowUsersUseCase listUsers,
    IGetWorkflowUseCase getWorkflow,
    ICreateWorkflowUseCase createWorkflow,
    IUpdateWorkflowUseCase updateWorkflow,
    IApproveWorkflowUseCase approveWorkflow,
    IRejectWorkflowUseCase rejectWorkflow,
    IAcceptWorkflowRejectionUseCase acceptRejection,
    IReturnRejectionToAssigneeUseCase returnToAssignee,
    IReturnRejectionToCreatorUseCase returnToCreator,
    IAddWorkflowAssigneeUseCase addAssignee,
    IReplaceWorkflowAssigneeUseCase replaceAssignee,
    IListWorkflowMessagesUseCase listMessages,
    IGetWorkflowAttachmentUseCase getAttachment) : ControllerBase
{
    private const long MaxCreateRequestBytes =
        (WorkflowErrors.MaxAttachments * WorkflowErrors.MaxAttachmentBytes) + (1024 * 1024);

    [HttpGet]
    [RequirePermission(Permissions.WorkflowUse)]
    [ProducesResponseType(typeof(PagedResponse<WorkflowSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<WorkflowSummaryResponse>>> List(
        [FromQuery] WorkflowsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await listWorkflows.ExecuteAsync(query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Erro ao buscar workflows", result);
    }

    [HttpGet("stats")]
    [RequirePermission(Permissions.WorkflowUse)]
    [ProducesResponseType(typeof(WorkflowStatsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<WorkflowStatsResponse>> Stats(CancellationToken cancellationToken)
    {
        var result = await getStats.ExecuteAsync(cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Erro ao buscar estatísticas", result);
    }

    [HttpGet("users")]
    [RequirePermission(Permissions.WorkflowUse)]
    [ProducesResponseType(typeof(IReadOnlyList<WorkflowUserResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<WorkflowUserResponse>>> Users(CancellationToken cancellationToken)
    {
        var result = await listUsers.ExecuteAsync(cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Erro ao buscar usuários", result);
    }

    [HttpGet("{id:long}")]
    [RequirePermission(Permissions.WorkflowUse)]
    [ProducesResponseType(typeof(WorkflowDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkflowDetailResponse>> GetById(ulong id, CancellationToken cancellationToken)
    {
        var result = await getWorkflow.ExecuteAsync(id, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Falha ao buscar workflow", result);
    }

    [HttpPost]
    [RequirePermission(Permissions.WorkflowUse)]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxCreateRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxCreateRequestBytes)]
    [ProducesResponseType(typeof(WorkflowSummaryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WorkflowSummaryResponse>> Create(
        [FromForm] CreateWorkflowRequest request,
        IFormFileCollection? attachments,
        CancellationToken cancellationToken)
    {
        var uploads = (attachments ?? new FormFileCollection())
            .Select(file => new WorkflowAttachmentUpload(
                Path.GetFileName(file.FileName),
                string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
                file.Length,
                file.OpenReadStream()))
            .ToList();

        var result = await createWorkflow.ExecuteAsync(request, uploads, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : ApiResults.Failure("Falha ao criar workflow", result);
    }

    [HttpPut("{id:long}")]
    [RequirePermission(Permissions.WorkflowUse)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(ulong id, UpdateWorkflowRequest request, CancellationToken cancellationToken)
    {
        var result = await updateWorkflow.ExecuteAsync(id, request, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao atualizar workflow", result);
    }

    [HttpPost("{id:long}/approve")]
    [RequirePermission(Permissions.WorkflowUse)]
    [ProducesResponseType(typeof(WorkflowActionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkflowActionResponse>> Approve(
        ulong id,
        WorkflowJustificationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await approveWorkflow.ExecuteAsync(id, request, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Falha ao aprovar workflow", result);
    }

    [HttpPost("{id:long}/reject")]
    [RequirePermission(Permissions.WorkflowUse)]
    [ProducesResponseType(typeof(WorkflowActionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkflowActionResponse>> Reject(
        ulong id,
        WorkflowJustificationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await rejectWorkflow.ExecuteAsync(id, request, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Falha ao rejeitar workflow", result);
    }

    [HttpPost("{id:long}/accept-rejection")]
    [RequirePermission(Permissions.WorkflowUse)]
    [ProducesResponseType(typeof(WorkflowActionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkflowActionResponse>> AcceptRejection(
        ulong id,
        WorkflowJustificationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await acceptRejection.ExecuteAsync(id, request, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Falha ao aceitar a rejeição", result);
    }

    [HttpPost("{id:long}/steps/{stepId:long}/return-to-assignee")]
    [RequirePermission(Permissions.WorkflowUse)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReturnToAssignee(
        ulong id,
        ulong stepId,
        WorkflowJustificationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await returnToAssignee.ExecuteAsync(id, stepId, request, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao devolver a revisão", result);
    }

    [HttpPost("{id:long}/steps/{stepId:long}/return-to-creator")]
    [RequirePermission(Permissions.WorkflowUse)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReturnToCreator(
        ulong id,
        ulong stepId,
        WorkflowJustificationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await returnToCreator.ExecuteAsync(id, stepId, request, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao devolver a rejeição", result);
    }

    [HttpGet("{id:long}/steps/{stepId:long}/messages")]
    [RequirePermission(Permissions.WorkflowUse)]
    [ProducesResponseType(typeof(IReadOnlyList<WorkflowMessageResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<WorkflowMessageResponse>>> Messages(
        ulong id,
        ulong stepId,
        CancellationToken cancellationToken)
    {
        var result = await listMessages.ExecuteAsync(id, stepId, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Falha ao carregar as mensagens", result);
    }

    [HttpPost("{id:long}/assignees")]
    [RequirePermission(Permissions.WorkflowUse)]
    [ProducesResponseType(typeof(WorkflowAssigneeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkflowAssigneeResponse>> AddAssignee(
        ulong id,
        AddWorkflowAssigneeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await addAssignee.ExecuteAsync(id, request, cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : ApiResults.Failure("Falha ao adicionar responsável", result);
    }

    [HttpPost("{id:long}/assignees/{userId:long}/replace")]
    [RequirePermission(Permissions.WorkflowUse)]
    [ProducesResponseType(typeof(WorkflowActionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkflowActionResponse>> ReplaceAssignee(
        ulong id,
        ulong userId,
        ReplaceWorkflowAssigneeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await replaceAssignee.ExecuteAsync(id, userId, request, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Falha ao substituir responsável", result);
    }

    [HttpGet("{id:long}/attachments/{attachmentId:long}")]
    [RequirePermission(Permissions.WorkflowUse)]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Attachment(ulong id, ulong attachmentId, CancellationToken cancellationToken)
    {
        var result = await getAttachment.ExecuteAsync(id, attachmentId, cancellationToken);

        if (result.IsFailed)
            return ApiResults.Failure("Falha ao baixar anexo", result);

        var download = result.Value;
        var contentType = MediaTypeHeaderValue.TryParse(download.ContentType, out _)
            ? download.ContentType
            : "application/octet-stream";

        return File(download.Content, contentType, download.FileName);
    }
}
