using FluentResults;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces.Production;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Production;

public sealed class DeleteLayoutUseCase(ILineLayoutRepository layouts) : IDeleteLayoutUseCase
{
    public async Task<Result> ExecuteAsync(ulong id, CancellationToken cancellationToken)
    {
        var layout = await layouts.GetByIdAsync(id, cancellationToken);
        if (layout is null)
            return Result.Fail(new NotFoundError(ProductionErrors.LayoutNotFound));

        layouts.Delete(layout);

        return await layouts.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok()
            : Result.Fail(ProductionErrors.NothingPersisted);
    }
}
