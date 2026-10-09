using IrisWeb.Domain.Entities;

namespace IrisWeb.Domain.Interfaces;

public interface IStationPresenceRepository : IRepository<StationPresence, long>
{
    Task<IReadOnlyList<StationPresence>> ListOverlappingAsync(
        DateTime from,
        DateTime to,
        string? workstation,
        string? line,
        string? registration,
        int limit,
        CancellationToken cancellationToken);

    /// <summary>Presenca aberta (LeftAt nulo) num posto, para fechar antes de abrir outra.</summary>
    Task<IReadOnlyList<StationPresence>> ListOpenAsync(
        string workstation,
        string line,
        CancellationToken cancellationToken);

    /// <summary>Presenca aberta ha mais que o corte, de qualquer posto — para o reaper de sessao presa.</summary>
    Task<IReadOnlyList<StationPresence>> ListOpenOlderThanAsync(
        DateTime cutoff,
        CancellationToken cancellationToken);
}
