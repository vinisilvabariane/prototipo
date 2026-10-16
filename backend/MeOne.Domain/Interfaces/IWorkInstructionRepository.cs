using MeOne.Domain.Entities;

namespace MeOne.Domain.Interfaces;

public interface IWorkInstructionRepository : IRepository<WorkInstruction, ulong>
{
    Task<bool> CodeExistsAsync(string normalizedCode, ulong? exceptId, CancellationToken cancellationToken);

    Task<int> CountAsync(string? search, CancellationToken cancellationToken);

    Task<IReadOnlyList<WorkInstruction>> SearchAsync(string? search, int skip, int take, CancellationToken cancellationToken);
}
