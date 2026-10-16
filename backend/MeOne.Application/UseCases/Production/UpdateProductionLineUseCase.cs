using FluentResults;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces.Production;
using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Production;

public sealed class UpdateProductionLineUseCase(IProductionLineRepository lines) : IUpdateProductionLineUseCase
{
    public async Task<Result> ExecuteAsync(ulong id, SaveProductionLineRequest request, CancellationToken cancellationToken)
    {
        var line = await lines.GetByIdAsync(id, cancellationToken);
        if (line is null)
            return Result.Fail(new NotFoundError(ProductionErrors.LineNotFound));

        if (await lines.NameExistsAsync(ProductionLine.NormalizeName(request.Name), id, cancellationToken))
            return Result.Fail(ProductionErrors.NameTaken(ProductionLine.NormalizeName(request.Name)));

        if (!line.Update(request.Name, request.Active))
            return Result.Ok();

        lines.Update(line);

        return await lines.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok()
            : Result.Fail(ProductionErrors.NothingPersisted);
    }
}
