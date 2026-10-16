using FluentValidation;
using MeOne.Application.Communication.OrgChart.Requests;

namespace MeOne.Application.Validators.OrgChart;

public sealed class CreateOrgPositionRequestValidator : AbstractValidator<CreateOrgPositionRequest>
{
    public CreateOrgPositionRequestValidator()
    {
        RuleFor(x => x.Title).ValidPositionTitle();
        RuleFor(x => x.DepartmentId).ValidPositionDepartment();
    }
}
