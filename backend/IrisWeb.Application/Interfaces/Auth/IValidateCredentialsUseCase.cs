using FluentResults;
using IrisWeb.Application.Communication.Auth.Requests;

namespace IrisWeb.Application.Interfaces.Auth;

public interface IValidateCredentialsUseCase
{
    Task<Result> ExecuteAsync(ValidateCredentialsRequest request, CancellationToken cancellationToken);
}
