using MeOne.Domain.Enums;
using MeOne.Domain.Errors;
using MeOne.Domain.Rules;

namespace MeOne.Domain.Entities;

public sealed record HelpdeskFieldTemplateDraft(
    string Name,
    HelpdeskFieldType Type,
    string Label,
    bool Required,
    IReadOnlyList<HelpdeskOption> Options);

public sealed class HelpdeskFieldTemplate : Entity<ulong>
{
    private HelpdeskFieldTemplate() { }

    public string Name { get; private set; } = string.Empty;

    public HelpdeskFieldType Type { get; private set; }

    public string Label { get; private set; } = string.Empty;

    public bool Required { get; private set; }

    public IReadOnlyList<HelpdeskOption> Options { get; private set; } = [];

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static string NormalizeName(string name) => name.Trim();

    public static string? Check(HelpdeskFieldTemplateDraft draft)
    {
        if (string.IsNullOrWhiteSpace(draft.Name))
            return HelpdeskErrors.NameRequired;

        if (draft.Name.Trim().Length > HelpdeskErrors.MaxNameLength)
            return HelpdeskErrors.FieldTooLong("O nome", HelpdeskErrors.MaxNameLength);

        if (draft.Type is not (HelpdeskFieldType.Text or HelpdeskFieldType.TextArea or HelpdeskFieldType.Number or HelpdeskFieldType.Select or HelpdeskFieldType.CardRadio))
            return HelpdeskErrors.TemplateTypeInvalid;

        var probe = new HelpdeskForm([new HelpdeskField("campo", draft.Type, draft.Label, draft.Required, null, draft.Options, null, null)]);

        return HelpdeskFormRules.Check(probe, resolution: false);
    }

    public static HelpdeskFieldTemplate Create(HelpdeskFieldTemplateDraft draft, DateTime now)
    {
        var template = new HelpdeskFieldTemplate { CreatedAt = now };
        template.Apply(draft, now);
        return template;
    }

    public void Update(HelpdeskFieldTemplateDraft draft, DateTime now) => Apply(draft, now);

    private void Apply(HelpdeskFieldTemplateDraft draft, DateTime now)
    {
        Name = NormalizeName(draft.Name);
        Type = draft.Type;
        Label = draft.Label.Trim();
        Required = draft.Required;
        Options = [.. draft.Options];
        UpdatedAt = now;
    }
}
