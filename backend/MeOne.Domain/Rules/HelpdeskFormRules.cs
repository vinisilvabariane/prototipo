using System.Text.RegularExpressions;
using MeOne.Domain.Entities;
using MeOne.Domain.Enums;
using MeOne.Domain.Errors;

namespace MeOne.Domain.Rules;

public static partial class HelpdeskFormRules
{
    [GeneratedRegex("^[a-z][a-z0-9_]*$")]
    private static partial Regex Slug();

    public static bool IsChoice(HelpdeskFieldType type) => type is HelpdeskFieldType.Select or HelpdeskFieldType.CardRadio;

    public static bool IsSpecial(HelpdeskFieldType type) =>
        type is HelpdeskFieldType.ProductionSpot or HelpdeskFieldType.LineStop or HelpdeskFieldType.Defects;

    public static string? Check(HelpdeskForm form, bool resolution)
    {
        if (form.Fields.Count > HelpdeskErrors.MaxFields)
            return HelpdeskErrors.FormTooLarge;

        var seen = new Dictionary<string, HelpdeskField>();
        var specials = new HashSet<HelpdeskFieldType>();

        foreach (var field in form.Fields)
        {
            if (CheckField(field) is { } problem)
                return problem;

            if (!seen.TryAdd(field.Id, field))
                return HelpdeskErrors.FieldIdDuplicated;

            if (IsSpecial(field.Type))
            {
                if (resolution)
                    return HelpdeskErrors.SpecialNotAllowedInResolution;

                if (!specials.Add(field.Type))
                    return HelpdeskErrors.SpecialDuplicated;
            }

            if (field.DependsOn is { } dependency)
            {
                if (dependency.FieldId == field.Id
                    || !seen.TryGetValue(dependency.FieldId, out var parent)
                    || !IsChoice(parent.Type))
                    return HelpdeskErrors.DependencyInvalid;

                if (dependency.Values.Count == 0 || dependency.Values.Any(value => parent.Options.All(option => option.Value != value)))
                    return HelpdeskErrors.DependencyValueInvalid;
            }
        }

        if ((specials.Contains(HelpdeskFieldType.LineStop) || specials.Contains(HelpdeskFieldType.Defects))
            && !specials.Contains(HelpdeskFieldType.ProductionSpot))
            return HelpdeskErrors.SpecialNeedsLocation;

        return null;
    }

    public static string? CheckSummary(HelpdeskForm form, IReadOnlyList<string> summaryFields)
    {
        if (summaryFields.Count > HelpdeskErrors.MaxSummaryFields || summaryFields.Distinct().Count() != summaryFields.Count)
            return HelpdeskErrors.SummaryFieldInvalid;

        return summaryFields.Any(id => form.Find(id) is not { } field || IsSpecial(field.Type))
            ? HelpdeskErrors.SummaryFieldInvalid
            : null;
    }

    public static IReadOnlyList<HelpdeskField> Visible(HelpdeskForm form, IReadOnlyDictionary<string, string> answers)
    {
        var visible = new List<HelpdeskField>();
        var visibleIds = new HashSet<string>();

        foreach (var field in form.Fields)
        {
            if (field.DependsOn is { } dependency
                && (!visibleIds.Contains(dependency.FieldId)
                    || !answers.TryGetValue(dependency.FieldId, out var parentAnswer)
                    || !dependency.Values.Contains(parentAnswer)))
                continue;

            visible.Add(field);
            visibleIds.Add(field.Id);
        }

        return visible;
    }

    private static string? CheckField(HelpdeskField field)
    {
        if (field.Id.Length > HelpdeskErrors.MaxFieldIdLength || !Slug().IsMatch(field.Id))
            return HelpdeskErrors.FieldIdInvalid;

        if (string.IsNullOrWhiteSpace(field.Label))
            return HelpdeskErrors.FieldLabelRequired;

        if (field.Label.Trim().Length > HelpdeskErrors.MaxLabelLength)
            return HelpdeskErrors.FieldTooLong("O título do campo", HelpdeskErrors.MaxLabelLength);

        if (field.Placeholder is { } placeholder && placeholder.Length > HelpdeskErrors.MaxPlaceholderLength)
            return HelpdeskErrors.FieldTooLong("A dica do campo", HelpdeskErrors.MaxPlaceholderLength);

        if (!IsChoice(field.Type))
            return field.Options.Count > 0 ? HelpdeskErrors.OptionsNotAllowed : null;

        if (field.Options.Count is 0 or > HelpdeskErrors.MaxOptions)
            return HelpdeskErrors.OptionsRequired;

        var values = new HashSet<string>();

        foreach (var option in field.Options)
        {
            if (string.IsNullOrWhiteSpace(option.Value) || string.IsNullOrWhiteSpace(option.Label) || option.Value.Length > HelpdeskErrors.MaxFieldIdLength)
                return HelpdeskErrors.OptionInvalid;

            if (option.Label.Length > HelpdeskErrors.MaxLabelLength)
                return HelpdeskErrors.FieldTooLong("O texto da opção", HelpdeskErrors.MaxLabelLength);

            if (!values.Add(option.Value))
                return HelpdeskErrors.OptionDuplicated;
        }

        return null;
    }
}
