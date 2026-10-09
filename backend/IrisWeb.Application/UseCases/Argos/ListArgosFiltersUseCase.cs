using FluentResults;
using IrisWeb.Application.Communication.Argos.Responses;
using IrisWeb.Application.Interfaces.Argos;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Application.UseCases.Argos;

public sealed class ListArgosFiltersUseCase(IArgosCycleRepository cycles) : IListArgosFiltersUseCase
{
    /// <summary>So olha os ultimos dias: estacao aposentada nao precisa aparecer para sempre.</summary>
    public const int LookbackDays = 90;

    public async Task<Result<ArgosFiltersResponse>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var since = DateTime.Now.AddDays(-LookbackDays);

        var stations = await cycles.ListStationsAsync(since, cancellationToken);
        var setups = await cycles.ListSetupsAsync(since, cancellationToken);

        return Result.Ok(new ArgosFiltersResponse(stations, setups));
    }
}
