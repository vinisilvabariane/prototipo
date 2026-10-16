using FluentResults;
using MeOne.Application.Communication.Suppliers.Requests;
using MeOne.Application.Communication.Suppliers.Responses;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Suppliers;
using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.Suppliers;

public sealed class ListSuppliersUseCase(ISupplierRepository suppliers, ICurrentActor currentActor) : IListSuppliersUseCase
{
    public async Task<Result<SupplierPageResponse>> ExecuteAsync(SuppliersQuery query, CancellationToken cancellationToken)
    {
        var search = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim();
        var today = SuppliersMapping.Today();

        var matching = await suppliers.ListAsync(search, query.Origin, query.MultiBranch, query.Criticality, cancellationToken);

        var filtered = query.Status switch
        {
            null => matching,
            Domain.Enums.SupplierStatusFilter.NotEvaluated => [.. matching.Where(supplier => supplier.CurrentQualification is null)],
            { } status => [.. matching.Where(supplier => supplier.CurrentQualification?.Matches(status, today) == true)],
        };

        var page = filtered
            .OrderBy(supplier => supplier.Name, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(supplier => supplier.Id)
            .Skip(query.Page * query.PageSize)
            .Take(query.PageSize)
            .Select(supplier => SuppliersMapping.ToSummary(supplier, today))
            .ToList();

        var profile = await currentActor.GetAccessProfileAsync(cancellationToken);

        return Result.Ok(new SupplierPageResponse(
            page,
            filtered.Count,
            query.Page,
            query.PageSize,
            profile?.Allows(Permissions.SuppliersRegister) ?? false));
    }
}
