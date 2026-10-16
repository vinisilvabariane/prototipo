using FluentResults;
using MeOne.Application.Communication.Suppliers.Requests;
using MeOne.Application.Communication.Suppliers.Responses;

namespace MeOne.Application.Interfaces.Suppliers;

public interface IAddSupplierSelfAssessmentUseCase
{
    Task<Result<SupplierSelfAssessmentResponse>> ExecuteAsync(ulong supplierId, SaveSelfAssessmentRequest request, CancellationToken cancellationToken);
}
