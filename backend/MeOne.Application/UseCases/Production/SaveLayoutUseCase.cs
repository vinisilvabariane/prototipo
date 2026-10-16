using FluentResults;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Production;
using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Production;

public sealed class SaveLayoutUseCase(ILineLayoutRepository layouts, ICurrentActor currentActor) : ISaveLayoutUseCase
{
    public async Task<Result> ExecuteAsync(ulong id, SaveLayoutRequest request, CancellationToken cancellationToken)
    {
        var actorId = await ProductionLookups.ActorIdAsync(currentActor, cancellationToken);
        if (actorId.IsFailed)
            return actorId.ToResult();

        var layout = await layouts.GetByIdAsync(id, cancellationToken);
        if (layout is null)
            return Result.Fail(new NotFoundError(ProductionErrors.LayoutNotFound));

        var posts = request.Posts
            .Select(post => new LayoutPostDraft(post.Id, post.Name, post.Kind, post.X, post.Y, post.Width, post.Height, post.Rotation, post.IsCritical, post.TargetPerHour, post.Operation))
            .ToList();

        var decorations = request.Decorations
            .Select(decoration => new LayoutDecorationDraft(decoration.Id, decoration.Kind, decoration.X, decoration.Y, decoration.Width, decoration.Height, decoration.Rotation, decoration.Label, decoration.Locked))
            .ToList();

        if (layout.Replace(posts, decorations, actorId.Value, DateTime.UtcNow) is { } problem)
            return Result.Fail(problem);

        layouts.Update(layout);

        return await layouts.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok()
            : Result.Fail(ProductionErrors.NothingPersisted);
    }
}
