using FluentResults;
using IrisWeb.Application.Communication.Argos.Requests;
using IrisWeb.Application.Communication.Argos.Responses;
using IrisWeb.Application.Interfaces.Argos;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Application.UseCases.Argos;

public sealed class ListArgosCyclesUseCase(IArgosCycleRepository cycles) : IListArgosCyclesUseCase
{
    public const int MaxRangeDays = 32;

    public const int DefaultPageSize = 50;

    public const int MaxPageSize = 500;

    public async Task<Result<ArgosCyclesResponse>> ExecuteAsync(
        ArgosCyclesQuery query,
        CancellationToken cancellationToken)
    {
        var filter = new ArgosCycleFilter(
            query.From!.Value,
            query.To!.Value,
            query.Shift,
            query.Result,
            ArgosMapping.Trimmed(query.Station),
            ArgosMapping.Trimmed(query.Setup));

        var pageSize = query.PageSize is null or < 1 ? DefaultPageSize : Math.Min(query.PageSize.Value, MaxPageSize);
        var requestedPage = query.Page is null or < 1 ? 1 : query.Page.Value;

        var (entries, total, page) = await cycles.PageAsync(filter, requestedPage, pageSize, cancellationToken);

        return Result.Ok(new ArgosCyclesResponse(
            [.. entries.Select(ArgosMapping.ToCycle)],
            total,
            page,
            pageSize));
    }
}
