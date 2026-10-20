using System.Text.RegularExpressions;
using MeOne.Domain.Errors;
using MeOne.Domain.Rules;

namespace MeOne.Domain.Entities;

public sealed record HelpdeskSectorDraft(
    string Name,
    string? Description,
    string ColorHex,
    string Icon,
    IReadOnlyList<string> Badges,
    bool Active,
    string? ExternalUrl,
    HelpdeskForm Form,
    HelpdeskForm ResolutionForm,
    IReadOnlyList<string> SummaryFields,
    IReadOnlyList<ulong> AttendantRoleIds);

public sealed partial class HelpdeskSector : Entity<ulong>
{
    [GeneratedRegex("^#[0-9a-fA-F]{6}$")]
    private static partial Regex Color();

    private HelpdeskSector() { }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string ColorHex { get; private set; } = "#2547D0";

    public string Icon { get; private set; } = "support";

    public IReadOnlyList<string> Badges { get; private set; } = [];

    public bool Active { get; private set; } = true;

    public string? ExternalUrl { get; private set; }

    public HelpdeskForm Form { get; private set; } = HelpdeskForm.Empty;

    public HelpdeskForm ResolutionForm { get; private set; } = HelpdeskForm.Empty;

    public IReadOnlyList<string> SummaryFields { get; private set; } = [];

    public IReadOnlyList<ulong> AttendantRoleIds { get; private set; } = [];

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public bool IsExternal => ExternalUrl is not null;

    public bool HasResolutionForm => !ResolutionForm.IsEmpty;

    public static string NormalizeName(string name) => name.Trim();

    public static string? Check(HelpdeskSectorDraft draft)
    {
        if (string.IsNullOrWhiteSpace(draft.Name))
            return HelpdeskErrors.NameRequired;

        if (draft.Name.Trim().Length > HelpdeskErrors.MaxNameLength)
            return HelpdeskErrors.FieldTooLong("O nome", HelpdeskErrors.MaxNameLength);

        if (draft.Description is { } description && description.Trim().Length > HelpdeskErrors.MaxDescriptionLength)
            return HelpdeskErrors.FieldTooLong("A descrição", HelpdeskErrors.MaxDescriptionLength);

        if (!Color().IsMatch(draft.ColorHex))
            return HelpdeskErrors.ColorInvalid;

        if (!HelpdeskIcons.Exists(draft.Icon))
            return HelpdeskErrors.IconInvalid;

        if (draft.Badges.Count > HelpdeskErrors.MaxBadges || draft.Badges.Any(badge => string.IsNullOrWhiteSpace(badge) || badge.Trim().Length > HelpdeskErrors.MaxBadgeLength))
            return $"Use até {HelpdeskErrors.MaxBadges} etiquetas de até {HelpdeskErrors.MaxBadgeLength} caracteres.";

        if (draft.ExternalUrl is { } url
            && (url.Length > HelpdeskErrors.MaxUrlLength
                || !Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri)
                || uri.Scheme is not ("http" or "https")))
            return HelpdeskErrors.UrlInvalid;

        if (HelpdeskFormRules.Check(draft.Form, resolution: false) is { } formProblem)
            return formProblem;

        if (HelpdeskFormRules.Check(draft.ResolutionForm, resolution: true) is { } resolutionProblem)
            return resolutionProblem;

        return HelpdeskFormRules.CheckSummary(draft.Form, draft.SummaryFields);
    }

    public static HelpdeskSector Create(HelpdeskSectorDraft draft, DateTime now)
    {
        var sector = new HelpdeskSector { CreatedAt = now };
        sector.Apply(draft, now);
        return sector;
    }

    public void Update(HelpdeskSectorDraft draft, DateTime now) => Apply(draft, now);

    public bool CanBeAttendedBy(IEnumerable<ulong> roleIds, bool isAdministrator) =>
        isAdministrator || AttendantRoleIds.Intersect(roleIds).Any();

    private void Apply(HelpdeskSectorDraft draft, DateTime now)
    {
        Name = NormalizeName(draft.Name);
        Description = string.IsNullOrWhiteSpace(draft.Description) ? null : draft.Description.Trim();
        ColorHex = draft.ColorHex.ToUpperInvariant();
        Icon = draft.Icon;
        Badges = [.. draft.Badges.Select(badge => badge.Trim())];
        Active = draft.Active;
        ExternalUrl = string.IsNullOrWhiteSpace(draft.ExternalUrl) ? null : draft.ExternalUrl.Trim();
        Form = ExternalUrl is null ? draft.Form : HelpdeskForm.Empty;
        ResolutionForm = ExternalUrl is null ? draft.ResolutionForm : HelpdeskForm.Empty;
        SummaryFields = ExternalUrl is null ? [.. draft.SummaryFields] : [];
        AttendantRoleIds = [.. draft.AttendantRoleIds.Distinct()];
        UpdatedAt = now;
    }
}
