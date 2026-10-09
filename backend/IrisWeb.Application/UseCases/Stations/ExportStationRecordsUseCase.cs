using FluentResults;
using IrisWeb.Application.Communication.Stations.Requests;
using IrisWeb.Application.Communication.Stations.Responses;
using IrisWeb.Application.Interfaces.Stations;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Application.UseCases.Stations;

public sealed class ExportStationRecordsUseCase(
    IStationPresenceRepository presences,
    IStationVacancyRepository vacancies,
    IStationUnknownRepository unknowns,
    IStationExportWriter writer) : IExportStationRecordsUseCase
{
    public async Task<Result<StationExportFileResponse>> ExecuteAsync(
        StationTimelineQuery query,
        CancellationToken cancellationToken)
    {
        var now = DateTime.Now;
        var (from, to) = ListStationTimelineUseCase.ResolveWindow(query, now);
        var openUntil = now < to ? now : to;

        var workstation = Trimmed(query.Workstation);
        var line = Trimmed(query.Line);
        var registration = Trimmed(query.Registration);
        var limit = ListStationTimelineUseCase.EntryLimit;

        var foundPresences = await presences.ListOverlappingAsync(
            from, to, workstation, line, registration, limit, cancellationToken);

        var foundVacancies = registration is null
            ? await vacancies.ListOverlappingAsync(from, to, workstation, line, limit, cancellationToken)
            : [];

        var foundUnknowns = registration is null
            ? await unknowns.ListOverlappingAsync(from, to, workstation, line, limit, cancellationToken)
            : [];

        var rows = foundPresences
            .Select(presence =>
            {
                var entry = StationMapping.ToEntry(presence, openUntil);
                return Row(entry, presence.CollaboratorName, presence.TrainingLevel, presence.AllocatedRegistration);
            })
            .Concat(foundVacancies.Select(vacancy => Row(StationMapping.ToEntry(vacancy, openUntil))))
            .Concat(foundUnknowns.Select(unknown => Row(StationMapping.ToEntry(unknown, openUntil))))
            .OrderBy(row => row.StartedAt)
            .ThenBy(row => row.Line, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.Workstation, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var truncated = foundPresences.Count >= limit
            || foundVacancies.Count >= limit
            || foundUnknowns.Count >= limit;

        var content = writer.Write(rows, from, to, truncated);

        return Result.Ok(new StationExportFileResponse(content, FileName(line, workstation, from, to)));
    }

    private static StationExportRow Row(
        StationTimelineEntryResponse entry,
        string? collaboratorName = null,
        string? trainingLevel = null,
        string? allocatedRegistration = null) =>
        new(
            entry.Kind,
            entry.Line,
            entry.Workstation,
            entry.Registration,
            collaboratorName,
            trainingLevel,
            allocatedRegistration,
            entry.StartedAt,
            entry.EndedAt,
            entry.DurationSeconds,
            entry.CandidateRegistration,
            entry.CandidateDistance);

    internal static string FileName(string? line, string? workstation, DateTime from, DateTime to)
    {
        var scope = line ?? workstation ?? "todas-as-linhas";
        var safe = string.Concat(scope.Select(c => Path.GetInvalidFileNameChars().Contains(c) || c == ' ' ? '-' : c));

        return $"registros-{safe}-{from:yyyyMMdd}-{to:yyyyMMdd}.xlsx";
    }

    private static string? Trimmed(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
