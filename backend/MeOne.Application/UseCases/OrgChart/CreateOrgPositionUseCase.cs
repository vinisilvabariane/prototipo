using FluentResults;
using MeOne.Application.Communication.OrgChart.Requests;
using MeOne.Application.Communication.OrgChart.Responses;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces.OrgChart;
using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.OrgChart;

public sealed class CreateOrgPositionUseCase(
    IOrgPositionRepository positions,
    IDepartmentRepository departments,
    IUserRepository users) : ICreateOrgPositionUseCase
{
    public async Task<Result<OrgPositionResponse>> ExecuteAsync(
        CreateOrgPositionRequest request,
        CancellationToken cancellationToken)
    {
        var department = await departments.GetByIdAsync(request.DepartmentId, cancellationToken);
        if (department is null)
            return Result.Fail<OrgPositionResponse>(new NotFoundError(DepartmentErrors.NotFound));

        var all = await positions.GetAllAsync(cancellationToken);

        if (request.ParentId is { } parentId)
        {
            if (all.All(position => position.Id != parentId))
                return Result.Fail<OrgPositionResponse>(new NotFoundError(OrgChartErrors.ParentNotFound));

            if (OrgChartHierarchy.DepthOf(all, parentId) + 1 > OrgChartHierarchy.MaxDepth)
                return Result.Fail<OrgPositionResponse>(OrgChartErrors.TooDeep);
        }

        User? holder = null;

        if (request.HolderId is { } holderId)
        {
            var found = await users.GetActiveByIdsAsync([holderId], cancellationToken);
            if (found.Count == 0)
                return Result.Fail<OrgPositionResponse>(new NotFoundError(OrgChartErrors.HolderNotFound));

            if (all.FirstOrDefault(position => position.HolderId == holderId) is { } occupied)
                return Result.Fail<OrgPositionResponse>(OrgChartErrors.HolderTaken(occupied.Title));

            holder = found[0];
        }

        var created = OrgPosition.Create(request.Title, department.Id, request.ParentId, holder?.Id);
        await positions.AddAsync(created, cancellationToken);

        return await positions.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok(OrgChartMapping.ToResponse(created, department.Name, holder))
            : Result.Fail<OrgPositionResponse>(OrgChartErrors.NothingPersisted);
    }
}
