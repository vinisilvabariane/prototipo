using MeOne.API.Auth;
using MeOne.API.Errors;
using MeOne.Application.Communication.Scopes.Requests;
using MeOne.Application.Communication.Scopes.Responses;
using MeOne.Application.Interfaces.Scopes;
using MeOne.Domain.Rules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeOne.API.Controllers;

[ApiController]
[Route("api/v1/scopes")]
public class ScopesController(
    IListScopesUseCase listScopes,
    IGetScopeUseCase getScope,
    ICreateScopeUseCase createScope,
    IUpdateScopeUseCase updateScope,
    IDeleteScopeUseCase deleteScope) : ControllerBase
{
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<ScopeResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ScopeResponse>>> List(CancellationToken cancellationToken)
    {
        var result = await listScopes.ExecuteAsync(cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Erro ao buscar escopos", result);
    }

    [HttpGet("{id:long}")]
    [Authorize]
    [ProducesResponseType(typeof(ScopeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ScopeResponse>> GetById(ulong id, CancellationToken cancellationToken)
    {
        var result = await getScope.ExecuteAsync(id, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Falha ao buscar escopo", result);
    }

    [HttpPost]
    [RequirePermission(Permissions.ScopesManage)]
    [ProducesResponseType(typeof(ScopeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ScopeResponse>> Create(CreateScopeRequest request, CancellationToken cancellationToken)
    {
        var result = await createScope.ExecuteAsync(request, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : ApiResults.Failure("Falha ao criar escopo", result);
    }

    [HttpPut("{id:long}")]
    [RequirePermission(Permissions.ScopesManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(ulong id, UpdateScopeRequest request, CancellationToken cancellationToken)
    {
        var result = await updateScope.ExecuteAsync(id, request, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao atualizar escopo", result);
    }

    [HttpDelete("{id:long}")]
    [RequirePermission(Permissions.ScopesManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(ulong id, CancellationToken cancellationToken)
    {
        var result = await deleteScope.ExecuteAsync(id, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao excluir escopo", result);
    }
}
