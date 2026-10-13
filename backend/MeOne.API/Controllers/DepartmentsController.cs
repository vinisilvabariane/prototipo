using MeOne.API.Auth;
using MeOne.API.Errors;
using MeOne.Application.Communication.Departments.Requests;
using MeOne.Application.Communication.Departments.Responses;
using MeOne.Application.Interfaces.Departments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeOne.API.Controllers;

[ApiController]
[Route("api/v1/departments")]
public class DepartmentsController(
    IListDepartmentNamesUseCase listNames,
    IListDepartmentsUseCase listDepartments,
    ICreateDepartmentUseCase createDepartment,
    IUpdateDepartmentUseCase updateDepartment,
    IDeleteDepartmentUseCase deleteDepartment) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<string>>> Names(CancellationToken cancellationToken)
    {
        var result = await listNames.ExecuteAsync(cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Erro ao buscar departamentos", result);
    }

    [HttpGet("all")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<DepartmentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<DepartmentResponse>>> All(CancellationToken cancellationToken)
    {
        var result = await listDepartments.ExecuteAsync(cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Erro ao buscar departamentos", result);
    }

    [HttpPost]
    [Authorize(Policy = AuthPolicies.MasterOnly)]
    [ProducesResponseType(typeof(DepartmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DepartmentResponse>> Create(
        CreateDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await createDepartment.ExecuteAsync(request, cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : ApiResults.Failure("Falha ao criar departamento", result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = AuthPolicies.MasterOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, UpdateDepartmentRequest request, CancellationToken cancellationToken)
    {
        var result = await updateDepartment.ExecuteAsync(id, request, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao atualizar departamento", result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = AuthPolicies.MasterOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await deleteDepartment.ExecuteAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao excluir departamento", result);
    }
}
