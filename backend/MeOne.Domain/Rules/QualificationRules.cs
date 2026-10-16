namespace MeOne.Domain.Rules;

public static class QualificationRules
{
    public const decimal MaxScore = 100;

    public static IReadOnlyList<int> AuditValidityMonths { get; } = [3, 6, 12];

    public static DateOnly SelfAssessmentExpiry(DateOnly assessedOn) => assessedOn.AddYears(1).AddDays(1);

    public static DateOnly AuditExpiry(DateOnly auditedOn, int validityMonths) => auditedOn.AddMonths(validityMonths);

    public static bool IsValidScore(decimal? score) => score is null or (>= 0 and <= MaxScore);
}
