using MeOne.Domain.Entities;
using MeOne.Domain.Interfaces;

namespace MeOne.Tests.Fakes;

public class FakeWorkInstructionRepository : InMemoryRepository<WorkInstruction, ulong>, IWorkInstructionRepository
{
    private ulong _lastStepIdentity;

    public Task<bool> CodeExistsAsync(string normalizedCode, ulong? exceptId, CancellationToken cancellationToken) =>
        Task.FromResult(Items.Any(instruction => instruction.Code == normalizedCode && instruction.Id != exceptId));

    public Task<int> CountAsync(string? search, CancellationToken cancellationToken) =>
        Task.FromResult(Matching(search).Count());

    public Task<IReadOnlyList<WorkInstruction>> SearchAsync(string? search, int skip, int take, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<WorkInstruction>>(
            [.. Matching(search).OrderBy(instruction => instruction.Title).ThenBy(instruction => instruction.Id).Skip(skip).Take(take)]);

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        foreach (var step in Items.SelectMany(instruction => instruction.Steps).Where(step => step.Id == 0))
            step.Id = ++_lastStepIdentity;

        return base.SaveChangesAsync(cancellationToken);
    }

    private IEnumerable<WorkInstruction> Matching(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return Items;

        var term = search.Trim();

        return Items.Where(instruction =>
            instruction.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            instruction.Code.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            instruction.Product.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            instruction.DocumentNumber.Contains(term, StringComparison.OrdinalIgnoreCase));
    }
}
