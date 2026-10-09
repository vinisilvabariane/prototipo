using FluentResults;
using IrisWeb.Application.Communication.Argos.Requests;
using IrisWeb.Application.Communication.Argos.Responses;
using IrisWeb.Application.Interfaces.Argos;
using IrisWeb.Domain.Enums;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Application.UseCases.Argos;

public sealed class GetArgosSummaryUseCase(IArgosCycleRepository cycles) : IGetArgosSummaryUseCase
{
    public async Task<Result<ArgosSummaryResponse>> ExecuteAsync(
        ArgosSummaryQuery query,
        CancellationToken cancellationToken)
    {
        var from = query.From!.Value;
        var to = query.To!.Value;
        var station = ArgosMapping.Trimmed(query.Station);
        var setup = ArgosMapping.Trimmed(query.Setup);

        var filter = new ArgosCycleFilter(from, to, query.Shift, query.Result, station, setup);

        var rows = await cycles.ListAsync(filter, cancellationToken);

        // As duas caixas de turno aparecem sempre: a contagem por turno ignora o filtro de turno.
        var rowsOfAnyShift = query.Shift == ArgosShiftFilter.All
            ? rows
            : await cycles.ListAsync(filter with { Shift = ArgosShiftFilter.All }, cancellationToken);

        // "Linha parada" nao pode depender da janela: olhar so ela daria "parada" toda virada de hora.
        var lastCycleAt = await cycles.LastRecordedAtAsync(station, setup, cancellationToken);

        return Result.Ok(ArgosMapping.ToSummary(
            from,
            to,
            query.Bucket,
            LocalSeconds(DateTime.Now),
            lastCycleAt,
            rows,
            rowsOfAnyShift));
    }

    // O contrato e hora local sem offset e sem fracao ("2026-10-01T09:42:31"); DateTime.Now sai
    // da serializacao com fuso e 7 casas decimais.
    private static DateTime LocalSeconds(DateTime value) =>
        new(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second);
}
