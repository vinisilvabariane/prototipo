using IrisWeb.Domain.Entities;

namespace IrisWeb.Domain.Interfaces;

public interface IStationVacancyRepository : IRepository<StationVacancy, long>
{
    Task<IReadOnlyList<StationVacancy>> ListOverlappingAsync(
        DateTime from,
        DateTime to,
        string? workstation,
        string? line,
        int limit,
        CancellationToken cancellationToken);

    /// <summary>Vacancia aberta (EndedAt nulo) num posto, para fechar antes de abrir outra.</summary>
    Task<IReadOnlyList<StationVacancy>> ListOpenAsync(
        string workstation,
        string line,
        CancellationToken cancellationToken);

    /// <summary>Vacancia aberta ha mais que o corte, de qualquer posto — para o reaper de sessao presa.</summary>
    Task<IReadOnlyList<StationVacancy>> ListOpenOlderThanAsync(
        DateTime cutoff,
        CancellationToken cancellationToken);
}
