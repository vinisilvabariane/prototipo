using MeOne.API.Auth;
using MeOne.API.Errors;
using MeOne.Application.Communication.Helpdesk.Requests;
using MeOne.Application.Communication.Helpdesk.Responses;
using MeOne.Application.Interfaces.Helpdesk;
using MeOne.Domain.Rules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeOne.API.Controllers;

[ApiController]
[Route("api/v1/helpdesk")]
public class HelpdeskSetupController(
    IListHelpdeskSectorsUseCase listSectors,
    IGetHelpdeskSectorUseCase getSector,
    ICreateHelpdeskSectorUseCase createSector,
    IUpdateHelpdeskSectorUseCase updateSector,
    IListHelpdeskFieldTemplatesUseCase listTemplates,
    ICreateHelpdeskFieldTemplateUseCase createTemplate,
    IUpdateHelpdeskFieldTemplateUseCase updateTemplate,
    IDeleteHelpdeskFieldTemplateUseCase deleteTemplate,
    IListHelpdeskRolesUseCase listRoles) : ControllerBase
{
    [HttpGet("sectors")]
    [Authorize]
    [ProducesResponseType(typeof(HelpdeskSectorListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<HelpdeskSectorListResponse>> Sectors([FromQuery] HelpdeskSectorsQuery query, CancellationToken cancellationToken)
    {
        var result = await listSectors.ExecuteAsync(query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Erro ao buscar setores", result);
    }

    [HttpGet("sectors/{id:long}")]
    [Authorize]
    [ProducesResponseType(typeof(HelpdeskSectorDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HelpdeskSectorDetailResponse>> SectorById(ulong id, CancellationToken cancellationToken)
    {
        var result = await getSector.ExecuteAsync(id, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Falha ao buscar setor", result);
    }

    [HttpPost("sectors")]
    [RequirePermission(Permissions.HelpdeskManage)]
    [ProducesResponseType(typeof(HelpdeskSectorDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<HelpdeskSectorDetailResponse>> CreateSector([FromBody] SaveHelpdeskSectorRequest request, CancellationToken cancellationToken)
    {
        var result = await createSector.ExecuteAsync(request, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(SectorById), new { id = result.Value.Id }, result.Value)
            : ApiResults.Failure("Falha ao criar setor", result);
    }

    [HttpPut("sectors/{id:long}")]
    [RequirePermission(Permissions.HelpdeskManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSector(ulong id, [FromBody] SaveHelpdeskSectorRequest request, CancellationToken cancellationToken)
    {
        var result = await updateSector.ExecuteAsync(id, request, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao salvar setor", result);
    }

    [HttpGet("field-templates")]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<HelpdeskFieldTemplateResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<HelpdeskFieldTemplateResponse>>> FieldTemplates(CancellationToken cancellationToken)
    {
        var result = await listTemplates.ExecuteAsync(cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Erro ao buscar modelos de campo", result);
    }

    [HttpPost("field-templates")]
    [RequirePermission(Permissions.HelpdeskManage)]
    [ProducesResponseType(typeof(HelpdeskFieldTemplateResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<HelpdeskFieldTemplateResponse>> CreateFieldTemplate([FromBody] SaveHelpdeskFieldTemplateRequest request, CancellationToken cancellationToken)
    {
        var result = await createTemplate.ExecuteAsync(request, cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : ApiResults.Failure("Falha ao criar modelo de campo", result);
    }

    [HttpPut("field-templates/{id:long}")]
    [RequirePermission(Permissions.HelpdeskManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateFieldTemplate(ulong id, [FromBody] SaveHelpdeskFieldTemplateRequest request, CancellationToken cancellationToken)
    {
        var result = await updateTemplate.ExecuteAsync(id, request, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao salvar modelo de campo", result);
    }

    [HttpDelete("field-templates/{id:long}")]
    [RequirePermission(Permissions.HelpdeskManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFieldTemplate(ulong id, CancellationToken cancellationToken)
    {
        var result = await deleteTemplate.ExecuteAsync(id, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao excluir modelo de campo", result);
    }

    [HttpGet("roles")]
    [RequirePermission(Permissions.HelpdeskManage)]
    [ProducesResponseType(typeof(IReadOnlyList<HelpdeskRoleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<HelpdeskRoleResponse>>> Roles(CancellationToken cancellationToken)
    {
        var result = await listRoles.ExecuteAsync(cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Erro ao buscar papéis", result);
    }
}
