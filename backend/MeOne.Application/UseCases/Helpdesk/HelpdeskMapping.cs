using System.Globalization;
using MeOne.Application.Communication.Helpdesk;
using MeOne.Application.Communication.Helpdesk.Requests;
using MeOne.Application.Communication.Helpdesk.Responses;
using MeOne.Domain.Entities;
using MeOne.Domain.Enums;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.Helpdesk;

internal static class HelpdeskMapping
{
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static HelpdeskOption ToDomain(HelpdeskOptionDto option) =>
        new((option.Value ?? string.Empty).Trim(), (option.Label ?? string.Empty).Trim(), Clean(option.Icon), Clean(option.Color));

    public static HelpdeskField ToDomain(HelpdeskFieldDto field) => new(
        (field.Id ?? string.Empty).Trim(),
        field.Type,
        (field.Label ?? string.Empty).Trim(),
        field.Required,
        Clean(field.Placeholder),
        [.. (field.Options ?? []).Select(ToDomain)],
        field.DependsOn is null ? null : new HelpdeskDependency((field.DependsOn.FieldId ?? string.Empty).Trim(), [.. (field.DependsOn.Values ?? [])]),
        field.TemplateId);

    public static HelpdeskForm ToDomain(HelpdeskFormDto? form) =>
        form?.Fields is null ? HelpdeskForm.Empty : new([.. form.Fields.Select(ToDomain)]);

    public static HelpdeskOptionDto ToDto(HelpdeskOption option) => new(option.Value, option.Label, option.Icon, option.Color);

    public static HelpdeskFieldDto ToDto(HelpdeskField field) => new(
        field.Id,
        field.Type,
        field.Label,
        field.Required,
        field.Placeholder,
        [.. field.Options.Select(ToDto)],
        field.DependsOn is null ? null : new HelpdeskDependencyDto(field.DependsOn.FieldId, field.DependsOn.Values),
        field.TemplateId);

    public static HelpdeskFormDto ToDto(HelpdeskForm form) => new([.. form.Fields.Select(ToDto)]);

    public static HelpdeskSectorDraft ToDraft(SaveHelpdeskSectorRequest request) => new(
        request.Name ?? string.Empty,
        request.Description,
        request.ColorHex ?? string.Empty,
        request.Icon ?? string.Empty,
        request.Badges ?? [],
        request.Active,
        Clean(request.ExternalUrl),
        ToDomain(request.Form),
        ToDomain(request.ResolutionForm),
        request.SummaryFields ?? [],
        request.AttendantRoleIds ?? []);

    public static HelpdeskFieldTemplateDraft ToDraft(SaveHelpdeskFieldTemplateRequest request) => new(
        request.Name ?? string.Empty,
        request.Type,
        request.Label ?? string.Empty,
        request.Required,
        [.. (request.Options ?? []).Select(ToDomain)]);

    public static HelpdeskFieldTemplateResponse ToResponse(HelpdeskFieldTemplate template) =>
        new(template.Id, template.Name, template.Type, template.Label, template.Required, [.. template.Options.Select(ToDto)]);

    public static HelpdeskSectorSummaryResponse ToSummary(HelpdeskSector sector, HelpdeskActor actor) => new(
        sector.Id,
        sector.Name,
        sector.Description,
        sector.ColorHex,
        sector.Icon,
        sector.Badges,
        sector.Active,
        sector.ExternalUrl,
        sector.Form.Fields.Count,
        actor.CanAttend(sector));

    public static HelpdeskSectorDetailResponse ToDetail(HelpdeskSector sector, HelpdeskActor actor) => new(
        sector.Id,
        sector.Name,
        sector.Description,
        sector.ColorHex,
        sector.Icon,
        sector.Badges,
        sector.Active,
        sector.ExternalUrl,
        ToDto(sector.Form),
        ToDto(sector.ResolutionForm),
        sector.SummaryFields,
        sector.AttendantRoleIds,
        actor.CanAttend(sector),
        actor.CanManage);

    public static IReadOnlyList<HelpdeskAnswerView> Views(
        HelpdeskForm form,
        IReadOnlyDictionary<string, string> answers,
        IReadOnlyDictionary<ulong, string> lineNames,
        string? postName,
        IReadOnlyCollection<string>? only = null)
    {
        var views = new List<HelpdeskAnswerView>();

        foreach (var field in HelpdeskFormRules.Visible(form, answers))
        {
            if (only is not null && !only.Contains(field.Id))
                continue;

            if (!answers.TryGetValue(field.Id, out var value))
                continue;

            DateTime? at = null;

            var display = field.Type switch
            {
                HelpdeskFieldType.Select or HelpdeskFieldType.CardRadio => field.Options.FirstOrDefault(option => option.Value == value)?.Label ?? value,
                HelpdeskFieldType.ProductionSpot => SpotText(value, lineNames, postName),
                HelpdeskFieldType.LineStop => value == HelpdeskAnswers.Stopped ? "Linha parada" : "Linha rodando",
                HelpdeskFieldType.Defects => $"{value} peça(s)",
                _ => value,
            };

            if (field.Type == HelpdeskFieldType.LineStop
                && value == HelpdeskAnswers.Stopped
                && answers.TryGetValue(field.Id + HelpdeskAnswers.TimeSuffix, out var time)
                && DateTime.TryParse(time, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var parsed))
                at = DateTime.SpecifyKind(parsed, DateTimeKind.Utc);

            views.Add(new(field.Label, field.Type, display, at));
        }

        return views;
    }

    private static string SpotText(string value, IReadOnlyDictionary<ulong, string> lineNames, string? postName)
    {
        var line = ulong.TryParse(value, CultureInfo.InvariantCulture, out var id) && lineNames.TryGetValue(id, out var name) ? name : value;

        return postName is null ? line : $"{line} · {postName}";
    }
}
