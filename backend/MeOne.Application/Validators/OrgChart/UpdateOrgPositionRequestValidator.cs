using FluentValidation;
using MeOne.Application.Communication.OrgChart.Requests;

namespace MeOne.Application.Validators.OrgChart;

public sealed class UpdateOrgPositionRequestValidator : AbstractValidator<UpdateOrgPositionRequest>
{
    public UpdateOrgPositionRequestValidator()
    {
        RuleFor(x => x.Title).ValidPositionTitle();
        RuleFor(x => x.DepartmentId).ValidPositionDepartment();
    }
}
