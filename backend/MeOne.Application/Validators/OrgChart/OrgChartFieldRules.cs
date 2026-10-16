using FluentValidation;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.OrgChart;

internal static class OrgChartFieldRules
{
    public static IRuleBuilderOptions<T, string> ValidPositionTitle<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage(OrgChartErrors.TitleRequired)
            .Must(title => title.Trim().Length <= OrgChartErrors.MaxTitleLength)
            .WithMessage(OrgChartErrors.FieldTooLong("O nome do cargo", OrgChartErrors.MaxTitleLength));

    public static IRuleBuilderOptions<T, int> ValidPositionDepartment<T>(this IRuleBuilder<T, int> rule) =>
        rule.GreaterThan(0).WithMessage(OrgChartErrors.DepartmentRequired);
}
