using FluentResults;
using IrisWeb.Application.Communication.Users.Requests;

namespace IrisWeb.Application.Interfaces.Users;

public interface IChangePasswordUseCase
{
    Task<Result> ExecuteAsync(ChangePasswordRequest request, CancellationToken cancellationToken);
}
