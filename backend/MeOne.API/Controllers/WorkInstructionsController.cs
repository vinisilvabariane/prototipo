using MeOne.API.Auth;
using MeOne.API.Errors;
using MeOne.Application.Communication.WorkInstructions.Dtos;
using MeOne.Application.Communication.WorkInstructions.Requests;
using MeOne.Application.Communication.WorkInstructions.Responses;
using MeOne.Application.Interfaces.WorkInstructions;
using MeOne.Domain.Errors;
using MeOne.Domain.Rules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace MeOne.API.Controllers;

[ApiController]
[Route("api/v1/work-instructions")]
public class WorkInstructionsController(
    IListWorkInstructionsUseCase listInstructions,
    IGetWorkInstructionUseCase getInstruction,
    IGetNextWorkInstructionCodeUseCase getNextCode,
    ICreateWorkInstructionUseCase createInstruction,
    IUpdateWorkInstructionUseCase updateInstruction,
    IDeleteWorkInstructionUseCase deleteInstruction,
    IGetWorkInstructionPdfUseCase getPdf,
    IGetWorkInstructionStepImageUseCase getStepImage) : ControllerBase
{
    private const long MaxRequestBytes =
        WorkInstructionErrors.MaxPdfBytes + (WorkInstructionErrors.MaxSteps * WorkInstructionErrors.MaxImageBytes) + (1024 * 1024);

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(WorkInstructionPageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WorkInstructionPageResponse>> List(
        [FromQuery] WorkInstructionsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await listInstructions.ExecuteAsync(query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Erro ao buscar instruções de trabalho", result);
    }

    [HttpGet("next-code")]
    [RequirePermission(Permissions.EngineeringManage)]
    [ProducesResponseType(typeof(WorkInstructionCodeResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<WorkInstructionCodeResponse>> NextCode(CancellationToken cancellationToken)
    {
        var result = await getNextCode.ExecuteAsync(cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Erro ao gerar o código", result);
    }

    [HttpGet("{id:long}")]
    [Authorize]
    [ProducesResponseType(typeof(WorkInstructionDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkInstructionDetailResponse>> GetById(ulong id, CancellationToken cancellationToken)
    {
        var result = await getInstruction.ExecuteAsync(id, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Falha ao buscar instrução de trabalho", result);
    }

    [HttpPost]
    [RequirePermission(Permissions.EngineeringManage)]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxRequestBytes, ValueCountLimit = 2000)]
    [ProducesResponseType(typeof(WorkInstructionSummaryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WorkInstructionSummaryResponse>> Create(
        [FromForm] SaveWorkInstructionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await createInstruction.ExecuteAsync(request, PdfUpload(), StepImages(request), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : ApiResults.Failure("Falha ao criar instrução de trabalho", result);
    }

    [HttpPut("{id:long}")]
    [RequirePermission(Permissions.EngineeringManage)]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxRequestBytes, ValueCountLimit = 2000)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        ulong id,
        [FromForm] SaveWorkInstructionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await updateInstruction.ExecuteAsync(id, request, PdfUpload(), StepImages(request), cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao atualizar instrução de trabalho", result);
    }

    [HttpDelete("{id:long}")]
    [RequirePermission(Permissions.EngineeringManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(ulong id, CancellationToken cancellationToken)
    {
        var result = await deleteInstruction.ExecuteAsync(id, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao excluir instrução de trabalho", result);
    }

    [HttpGet("{id:long}/pdf")]
    [Authorize]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Pdf(ulong id, CancellationToken cancellationToken)
    {
        var result = await getPdf.ExecuteAsync(id, cancellationToken);

        return result.IsSuccess ? Inline(result.Value) : ApiResults.Failure("Falha ao abrir o PDF", result);
    }

    [HttpGet("{id:long}/steps/{stepId:long}/image")]
    [Authorize]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StepImage(ulong id, ulong stepId, CancellationToken cancellationToken)
    {
        var result = await getStepImage.ExecuteAsync(id, stepId, cancellationToken);

        return result.IsSuccess ? Inline(result.Value) : ApiResults.Failure("Falha ao abrir a imagem", result);
    }

    private IActionResult Inline(WorkInstructionDownload download)
    {
        Response.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline")
        {
            FileNameStar = download.FileName,
        }.ToString();

        return File(download.Content, download.ContentType);
    }

    private WorkInstructionUpload? PdfUpload() => Upload(Request.Form.Files.GetFile("pdf"));

    private List<WorkInstructionUpload?> StepImages(SaveWorkInstructionRequest request) =>
    [
        .. Enumerable.Range(0, request.Steps?.Length ?? 0)
            .Select(index => Upload(Request.Form.Files.GetFile($"stepImages[{index}]"))),
    ];

    private static WorkInstructionUpload? Upload(IFormFile? file) => file is null
        ? null
        : new WorkInstructionUpload(Path.GetFileName(file.FileName), file.Length, file.OpenReadStream());
}
