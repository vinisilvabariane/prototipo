using FluentResults;


namespace MeOne.Application.Interfaces.Suppliers;

public interface IRemoveSupplierSelfAssessmentUseCase
{
    Task<Result> ExecuteAsync(ulong supplierId, ulong itemId, CancellationToken cancellationToken);
}
