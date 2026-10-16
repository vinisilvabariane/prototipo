using MeOne.Application.Communication.OrgChart.Responses;
using MeOne.Domain.Entities;

namespace MeOne.Application.UseCases.OrgChart;

internal static class OrgChartMapping
{
    public static OrgPositionResponse ToResponse(OrgPosition position, string department, User? holder) => new(
        position.Id,
        position.Title,
        position.ParentId,
        position.DepartmentId,
        department,
        holder is null ? null : new OrgHolderResponse(holder.Id, holder.Name, holder.Registration, holder.Email, holder.Active));
}
