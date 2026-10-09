using IrisWeb.Domain.Entities;

namespace IrisWeb.Domain.Interfaces;

public interface IArgosCycleRepository : IRepository<ArgosCycle, long>
{
    /// <summary>Todas as pecas do filtro (janela, turno, resultado, estacao e setup), da mais antiga a mais nova.</summary>
    Task<IReadOnlyList<ArgosCycle>> ListAsync(ArgosCycleFilter filter, CancellationToken cancellationToken);

    /// <summary>Uma pagina do filtro, da mais nova a mais antiga. Pagina alem do fim vira a ultima.</summary>
    Task<(IReadOnlyList<ArgosCycle> Entries, int Total, int Page)> PageAsync(
        ArgosCycleFilter filter,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    /// <summary>Momento da ultima peca de qualquer resultado, ignorando janela e turno.</summary>
    Task<DateTime?> LastRecordedAtAsync(string? station, string? setup, CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> ListStationsAsync(DateTime since, CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> ListSetupsAsync(DateTime since, CancellationToken cancellationToken);
}
