using MeOne.Domain.Entities;

namespace MeOne.Domain.Interfaces;

public interface IHelpdeskFieldTemplateRepository : IRepository<HelpdeskFieldTemplate, ulong>
{
    Task<bool> NameExistsAsync(string name, ulong? exceptId, CancellationToken cancellationToken);

    Task<IReadOnlyList<HelpdeskFieldTemplate>> ListAsync(CancellationToken cancellationToken);
}
