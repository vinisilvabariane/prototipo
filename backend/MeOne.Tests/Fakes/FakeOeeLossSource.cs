using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;

namespace MeOne.Tests.Fakes;

public sealed class FakeOeeLossSource : IOeeLossSource
{
    public OeeLosses Losses { get; set; } = OeeLosses.None;

    public Task<OeeLosses> GetAsync(ulong lineId, DateTime from, DateTime to, CancellationToken cancellationToken) =>
        Task.FromResult(Losses);
}
