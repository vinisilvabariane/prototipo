using FluentResults;
using MeOne.Application.Communication.Suppliers.Requests;
using MeOne.Application.Communication.Suppliers.Responses;
using MeOne.Application.Communication.Suppliers.Dtos;

namespace MeOne.Application.Interfaces.Suppliers;

public interface IAddSupplierDocumentUseCase
{
    Task<Result<SupplierDocumentResponse>> ExecuteAsync(
        ulong supplierId,
        AddSupplierDocumentRequest request,
        SupplierFileUpload? file,
        CancellationToken cancellationToken);
}
