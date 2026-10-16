using MeOne.API.Auth;
using MeOne.API.Errors;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Application.Communication.Production.Responses;
using MeOne.Application.Interfaces.Production;
using MeOne.Domain.Rules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeOne.API.Controllers;

[ApiController]
[Route("api/v1/production/runs")]
public class ProductionRunsController(
    IListRunsUseCase listRuns,
    IGetRunUseCase getRun,
    ICreateRunUseCase createRun,
    IListRunCandidatesUseCase listCandidates,
    IAllocatePostUseCase allocatePost,
    IReleasePostUseCase releasePost,
    IReleaseAllPostsUseCase releaseAll,
    ICopyPreviousAllocationsUseCase copyPrevious,
    IStartRunUseCase startRun,
    ICloseRunUseCase closeRun,
    ICancelRunUseCase cancelRun,
    IRecordRunOutputUseCase recordOutput) : ControllerBase
{
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(RunPageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RunPageResponse>> List([FromQuery] RunsQuery query, CancellationToken cancellationToken)
    {
        var result = await listRuns.ExecuteAsync(query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Erro ao buscar produções", result);
    }

    [HttpGet("{id:long}")]
    [Authorize]
    [ProducesResponseType(typeof(RunDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RunDetailResponse>> GetById(ulong id, CancellationToken cancellationToken)
    {
        var result = await getRun.ExecuteAsync(id, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Falha ao buscar produção", result);
    }

    [HttpGet("{id:long}/candidates")]
    [RequirePermission(Permissions.ProductionSupervise)]
    [ProducesResponseType(typeof(IReadOnlyList<RunCandidateResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<RunCandidateResponse>>> Candidates(
        ulong id,
        [FromQuery] CandidatesQuery query,
        CancellationToken cancellationToken)
    {
        var result = await listCandidates.ExecuteAsync(id, query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Erro ao buscar pessoas", result);
    }

    [HttpPost]
    [RequirePermission(Permissions.ProductionSupervise)]
    [ProducesResponseType(typeof(RunSummaryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RunSummaryResponse>> Create([FromBody] CreateRunRequest request, CancellationToken cancellationToken)
    {
        var result = await createRun.ExecuteAsync(request, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : ApiResults.Failure("Falha ao preparar a produção", result);
    }

    [HttpPost("{id:long}/posts/{postId:long}/allocation")]
    [RequirePermission(Permissions.ProductionSupervise)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Allocate(ulong id, ulong postId, [FromBody] AllocatePostRequest request, CancellationToken cancellationToken)
    {
        var result = await allocatePost.ExecuteAsync(id, postId, request, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao alocar a pessoa", result);
    }

    [HttpDelete("{id:long}/posts/{postId:long}/allocation")]
    [RequirePermission(Permissions.ProductionSupervise)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Release(ulong id, ulong postId, CancellationToken cancellationToken)
    {
        var result = await releasePost.ExecuteAsync(id, postId, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao liberar o posto", result);
    }

    [HttpDelete("{id:long}/allocations")]
    [RequirePermission(Permissions.ProductionSupervise)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReleaseAll(ulong id, CancellationToken cancellationToken)
    {
        var result = await releaseAll.ExecuteAsync(id, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao liberar os postos", result);
    }

    [HttpPost("{id:long}/allocations/copy-previous")]
    [RequirePermission(Permissions.ProductionSupervise)]
    [ProducesResponseType(typeof(CopyAllocationsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CopyAllocationsResponse>> CopyPrevious(ulong id, CancellationToken cancellationToken)
    {
        var result = await copyPrevious.ExecuteAsync(id, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Falha ao repetir a alocação anterior", result);
    }

    [HttpPost("{id:long}/start")]
    [RequirePermission(Permissions.ProductionSupervise)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Start(ulong id, CancellationToken cancellationToken)
    {
        var result = await startRun.ExecuteAsync(id, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao iniciar a produção", result);
    }

    [HttpPost("{id:long}/close")]
    [RequirePermission(Permissions.ProductionSupervise)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Close(ulong id, CancellationToken cancellationToken)
    {
        var result = await closeRun.ExecuteAsync(id, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao encerrar a produção", result);
    }

    [HttpDelete("{id:long}")]
    [RequirePermission(Permissions.ProductionSupervise)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(ulong id, CancellationToken cancellationToken)
    {
        var result = await cancelRun.ExecuteAsync(id, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao cancelar a produção", result);
    }

    [HttpPost("{id:long}/outputs")]
    [RequirePermission(Permissions.ProductionExecute)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordOutput(ulong id, [FromBody] RecordOutputRequest request, CancellationToken cancellationToken)
    {
        var result = await recordOutput.ExecuteAsync(id, request, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao lançar a produção", result);
    }
}
