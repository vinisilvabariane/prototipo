using IrisWeb.Domain.Entities;

namespace IrisWeb.Domain.Interfaces;

public interface IStationUnknownRepository : IRepository<StationUnknown, long>
{
    Task<IReadOnlyList<StationUnknown>> ListOverlappingAsync(
        DateTime from,
        DateTime to,
        string? workstation,
        string? line,
        int limit,
        CancellationToken cancellationToken);

    /// <summary>Rosto desconhecido aberto (EndedAt nulo) num posto, para fechar antes de abrir outro.</summary>
    Task<IReadOnlyList<StationUnknown>> ListOpenAsync(
        string workstation,
        string line,
        CancellationToken cancellationToken);

    /// <summary>Rosto desconhecido aberto ha mais que o corte, de qualquer posto — para o reaper de sessao presa.</summary>
    Task<IReadOnlyList<StationUnknown>> ListOpenOlderThanAsync(
        DateTime cutoff,
        CancellationToken cancellationToken);
}
