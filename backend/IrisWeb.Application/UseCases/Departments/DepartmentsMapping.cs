using IrisWeb.Application.Communication.Departments.Responses;
using IrisWeb.Domain.Entities;

namespace IrisWeb.Application.UseCases.Departments;

internal static class DepartmentsMapping
{
    public static DepartmentResponse ToResponse(Department department) => new(department.Id, department.Name);
}
