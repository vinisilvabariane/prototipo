using MeOne.Domain.Entities;
using MeOne.Domain.Enums;
using MeOne.Domain.Interfaces;

namespace MeOne.Tests.Fakes;

public sealed class FakeSupplierRepository : InMemoryRepository<Supplier, ulong>, ISupplierRepository
{
    private ulong _lastChildIdentity;

    public Task<bool> SapCodeExistsAsync(string normalizedCode, MultiBranch multiBranch, ulong? exceptId, CancellationToken cancellationToken) =>
        Task.FromResult(Items.Any(supplier =>
            supplier.SapCode == normalizedCode && supplier.MultiBranch == multiBranch && supplier.Id != exceptId));

    public Task<IReadOnlyList<Supplier>> ListAsync(
        string? search,
        SupplierOrigin? origin,
        MultiBranch? multiBranch,
        SupplierCriticality? criticality,
        CancellationToken cancellationToken)
    {
        var term = string.IsNullOrWhiteSpace(search) ? null : search.Trim();

        IReadOnlyList<Supplier> found =
        [
            .. Items
                .Where(supplier => term is null
                    || supplier.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || supplier.SapCode.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || (supplier.CorporateName?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
                    || (supplier.TaxId?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false))
                .Where(supplier => origin is null || supplier.Origin == origin)
                .Where(supplier => multiBranch is null || supplier.MultiBranch == multiBranch)
                .Where(supplier => criticality is null || supplier.Criticality == criticality),
        ];

        return Task.FromResult(found);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        foreach (var supplier in Items)
        {
            foreach (var tool in supplier.Tools.Where(item => item.Id == 0))
                tool.Id = ++_lastChildIdentity;

            foreach (var material in supplier.Materials.Where(item => item.Id == 0))
                material.Id = ++_lastChildIdentity;

            foreach (var assessment in supplier.SelfAssessments.Where(item => item.Id == 0))
                assessment.Id = ++_lastChildIdentity;

            foreach (var audit in supplier.Audits.Where(item => item.Id == 0))
                audit.Id = ++_lastChildIdentity;

            foreach (var document in supplier.Documents.Where(item => item.Id == 0))
                document.Id = ++_lastChildIdentity;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
