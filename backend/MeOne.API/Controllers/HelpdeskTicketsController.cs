using MeOne.API.Errors;
using MeOne.Application.Communication.Helpdesk.Requests;
using MeOne.Application.Communication.Helpdesk.Responses;
using MeOne.Application.Interfaces.Helpdesk;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeOne.API.Controllers;

[ApiController]
[Route("api/v1/helpdesk")]
public class HelpdeskTicketsController(
    IListHelpdeskLocationsUseCase listLocations,
    IOpenHelpdeskTicketUseCase openTicket,
    IListHelpdeskTicketsUseCase listTickets,
    IGetHelpdeskTicketUseCase getTicket,
    IStartHelpdeskTicketUseCase startTicket,
    IResolveHelpdeskTicketUseCase resolveTicket,
    IRejectHelpdeskTicketUseCase rejectTicket) : ControllerBase
{
    [HttpGet("locations")]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<HelpdeskLocationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<HelpdeskLocationResponse>>> Locations(CancellationToken cancellationToken)
    {
        var result = await listLocations.ExecuteAsync(cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Erro ao buscar linhas e postos", result);
    }

    [HttpGet("tickets")]
    [Authorize]
    [ProducesResponseType(typeof(HelpdeskTicketPageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<HelpdeskTicketPageResponse>> Tickets([FromQuery] HelpdeskTicketsQuery query, CancellationToken cancellationToken)
    {
        var result = await listTickets.ExecuteAsync(query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Erro ao buscar chamados", result);
    }

    [HttpGet("tickets/{id:long}")]
    [Authorize]
    [ProducesResponseType(typeof(HelpdeskTicketDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HelpdeskTicketDetailResponse>> TicketById(ulong id, CancellationToken cancellationToken)
    {
        var result = await getTicket.ExecuteAsync(id, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Falha ao buscar chamado", result);
    }

    [HttpPost("tickets")]
    [Authorize]
    [ProducesResponseType(typeof(HelpdeskTicketSummaryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HelpdeskTicketSummaryResponse>> Open([FromBody] OpenHelpdeskTicketRequest request, CancellationToken cancellationToken)
    {
        var result = await openTicket.ExecuteAsync(request, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(TicketById), new { id = result.Value.Id }, result.Value)
            : ApiResults.Failure("Falha ao abrir chamado", result);
    }

    [HttpPost("tickets/{id:long}/start")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Start(ulong id, CancellationToken cancellationToken)
    {
        var result = await startTicket.ExecuteAsync(id, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao assumir chamado", result);
    }

    [HttpPost("tickets/{id:long}/resolve")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Resolve(ulong id, [FromBody] ResolveHelpdeskTicketRequest request, CancellationToken cancellationToken)
    {
        var result = await resolveTicket.ExecuteAsync(id, request, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao finalizar chamado", result);
    }

    [HttpPost("tickets/{id:long}/reject")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reject(ulong id, [FromBody] RejectHelpdeskTicketRequest request, CancellationToken cancellationToken)
    {
        var result = await rejectTicket.ExecuteAsync(id, request, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao recusar chamado", result);
    }
}
