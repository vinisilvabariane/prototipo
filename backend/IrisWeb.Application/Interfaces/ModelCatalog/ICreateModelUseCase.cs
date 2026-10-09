using FluentResults;
using IrisWeb.Application.Communication.ModelCatalog.Requests;
using IrisWeb.Application.Communication.ModelCatalog.Responses;

namespace IrisWeb.Application.Interfaces.ModelCatalog;

public interface ICreateModelUseCase
{
    Task<Result<CatalogModelResponse>> ExecuteAsync(CreateModelRequest request, CancellationToken cancellationToken);
}
