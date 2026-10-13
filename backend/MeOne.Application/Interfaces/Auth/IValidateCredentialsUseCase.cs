using FluentResults;
using MeOne.Application.Communication.Auth.Requests;

namespace MeOne.Application.Interfaces.Auth;

public interface IValidateCredentialsUseCase
{
    Task<Result> ExecuteAsync(ValidateCredentialsRequest request, CancellationToken cancellationToken);
}
