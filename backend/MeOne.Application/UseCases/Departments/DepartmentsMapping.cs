using MeOne.Application.Communication.Departments.Responses;
using MeOne.Domain.Entities;

namespace MeOne.Application.UseCases.Departments;

internal static class DepartmentsMapping
{
    public static DepartmentResponse ToResponse(Department department) => new(department.Id, department.Name);
}
