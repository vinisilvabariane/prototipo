using FluentResults;
using IrisWeb.Application.Communication.Stations.Requests;
using IrisWeb.Application.Interfaces.Stations;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Application.UseCases.Stations;

public sealed class CloseUnknownUseCase(IStationUnknownRepository unknowns) : ICloseUnknownUseCase
{
    public async Task<Result> ExecuteAsync(
        long unknownId,
        CloseUnknownRequest request,
        CancellationToken cancellationToken)
    {
        if (unknownId <= 0)
            return Result.Fail(StationErrors.InvalidSession);

        var unknown = await unknowns.GetByIdAsync(unknownId, cancellationToken);

        if (unknown is null || !unknown.Close(request.EndedSecondsAgo, request.CandidateRegistration, request.CandidateDistance))
            return Result.Ok();

        unknowns.Update(unknown);
        await unknowns.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
