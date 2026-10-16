using FluentResults;
using MeOne.Application.Communication.Suppliers.Requests;

namespace MeOne.Application.Interfaces.Suppliers;

public interface IUpdateSupplierSelfAssessmentUseCase
{
    Task<Result> ExecuteAsync(ulong supplierId, ulong itemId, SaveSelfAssessmentRequest request, CancellationToken cancellationToken);
}
