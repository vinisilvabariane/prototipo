using MeOne.Domain.Rules;

namespace MeOne.Domain.Interfaces;

public interface IOeeLossSource
{
    Task<OeeLosses> GetAsync(ulong lineId, DateTime from, DateTime to, CancellationToken cancellationToken);
}
